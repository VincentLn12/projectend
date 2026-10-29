using System.Text;
using System.Threading.RateLimiting;
using ComSci.Api.Middleware;
using ComSci.Api.Services;
using ComSci.Application.Abstractions;
using ComSci.Application.Options;
using ComSci.Infrastructure;
using ComSci.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
