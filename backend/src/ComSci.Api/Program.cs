using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using ComSci.Api.Middleware;
using ComSci.Api.Services;
using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Options;
using ComSci.Infrastructure;
using ComSci.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// กุญแจ JWT ต้องอยู่ใน user-secrets ไม่ใส่ใน appsettings ที่ขึ้น git
JwtOptions? jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
JwtOptions jwtOptions = jwtSettings ?? new JwtOptions();
if (jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be set (at least 32 characters), e.g. with dotnet user-secrets.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));

// ไฟล์ที่ผู้ใช้อัปโหลดต้องอยู่นอกโฟลเดอร์แอป ไม่งั้น deploy ทับแล้วไฟล์หายหมดทั้งที่ฐานข้อมูลยังอ้างถึง
if (!builder.Environment.IsDevelopment())
{
    string configuredStorageRoot = builder.Configuration[StorageOptions.SectionName + ":RootPath"] ?? string.Empty;
    bool isAbsolute = Path.IsPathRooted(configuredStorageRoot);
    string appFolderWithSeparator = Path.TrimEndingDirectorySeparator(builder.Environment.ContentRootPath) + Path.DirectorySeparatorChar;
    bool isInsideAppFolder = isAbsolute
        && Path.GetFullPath(configuredStorageRoot).StartsWith(appFolderWithSeparator, StringComparison.OrdinalIgnoreCase);
    if (!isAbsolute || isInsideAppFolder)
    {
        throw new InvalidOperationException(
            "Storage:RootPath must be an absolute folder outside the application folder (for example D:/comsci-uploads), and it must be backed up with the database.");
    }
}

// ข้างหลัง reverse proxy (nginx, IIS) ที่อยู่ของผู้เรียกที่แท้จริงอยู่ในหัว X-Forwarded-For
// เชื่อถือเฉพาะ proxy ที่ระบุ (ค่าเริ่มต้นคือเครื่องเดียวกัน) ไม่งั้นทุกคนจะมี IP เดียวกันและโดน rate limit รวมกัน
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;

    string[]? knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>();
    if (knownProxies != null)
    {
        foreach (string proxyAddress in knownProxies)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxyAddress));
        }
    }
});

// path แบบ relative อิงจากโฟลเดอร์ของแอป
builder.Services.PostConfigure<StorageOptions>(options =>
{
    options.RootPath = Path.GetFullPath(options.RootPath, builder.Environment.ContentRootPath);
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

string[]? allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
if (allowedOrigins == null)
{
    allowedOrigins = new string[0];
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
    {
        string clientAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // 10 ครั้งต่อนาทีต่อ IP กันการเดารหัสผ่าน
        return RateLimitPartition.GetFixedWindowLimiter(clientAddress, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1)
        });
    });

    // ใครก็สมัครบัญชีได้ และไฟล์เปิดดูสาธารณะ จึงจำกัดการอัปโหลดต่อคน ไม่ให้ใช้เป็นที่ฝากไฟล์ฟรี
    options.AddPolicy("upload", httpContext =>
    {
        if (httpContext.User.IsInRole(Roles.Admin))
        {
            return RateLimitPartition.GetNoLimiter("admin");
        }

        string? userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        string partitionKey = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromHours(1)
        });
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ComSci API", Version = "v1" });

    OpenApiSecurityScheme bearerScheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Log in via POST /api/auth/login, then paste the accessToken here (without the word Bearer)."
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);

    OpenApiSecurityScheme bearerReference = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    OpenApiSecurityRequirement requirement = new OpenApiSecurityRequirement();
    requirement.Add(bearerReference, new List<string>());
    options.AddSecurityRequirement(requirement);
});

WebApplication app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

app.UseForwardedHeaders();

app.Use(async (httpContext, next) =>
{
    httpContext.Response.Headers.XContentTypeOptions = "nosniff";
    await next();
});

// ลำดับ middleware มีผล อย่าสลับ
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.ConfigObject.PersistAuthorization = true;
    });
}

// Development serves plain http so the Angular dev server proxy can reach it whichever launch profile is used
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseAuthentication();
// หลัง UseAuthentication เพื่อให้นโยบายรู้ว่าผู้เรียกคือใคร (นโยบาย upload แยกตามผู้ใช้และยกเว้นแอดมิน)
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
