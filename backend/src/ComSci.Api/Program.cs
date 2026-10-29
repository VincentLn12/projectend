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

// =====================================================================
// 1) ค่าตั้งค่า
// =====================================================================

// กุญแจ JWT ต้องตั้งไว้ใน user-secrets (ห้ามใส่ในไฟล์ appsettings ที่ขึ้น git)
JwtOptions? jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
JwtOptions jwtOptions = jwtSettings ?? new JwtOptions();
if (jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be set (at least 32 characters), e.g. with dotnet user-secrets.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));

// path ของโฟลเดอร์เก็บไฟล์แบบ relative ให้อิงจากโฟลเดอร์ของแอป
builder.Services.PostConfigure<StorageOptions>(options =>
{
    options.RootPath = Path.GetFullPath(options.RootPath, builder.Environment.ContentRootPath);
});

// =====================================================================
// 2) Service ของระบบ
// =====================================================================

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddInfrastructure(builder.Configuration);

// แปลง exception ทางธุรกิจ (NotFound/BadRequest/Conflict) เป็น response ที่เหมาะสม
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// =====================================================================
// 3) ระบบยืนยันตัวตน (JWT)
// =====================================================================

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

// =====================================================================
// 4) CORS: อนุญาตเฉพาะเว็บ Angular ตามที่ตั้งไว้ใน Cors:Origins
// =====================================================================

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

// =====================================================================
// 5) จำกัดอัตราเรียก endpoint ล็อกอิน/สมัคร/refresh: 10 ครั้งต่อนาทีต่อ IP กันการเดารหัสผ่าน
// =====================================================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
    {
        string clientAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(clientAddress, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1)
        });
    });
});

// =====================================================================
// 6) Swagger (ใช้ทดสอบ API ที่ /swagger)
// =====================================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ComSci API", Version = "v1" });

    // ปุ่ม Authorize: ล็อกอินที่ POST /api/auth/login แล้ววาง accessToken
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

// =====================================================================
// สร้างแอปและกำหนดลำดับ middleware (ลำดับมีผล อย่าสลับ)
// =====================================================================

WebApplication app = builder.Build();

// สร้างข้อมูลตั้งต้น (role, ผู้ดูแลคนแรก, ตั้งค่าเว็บ) ถ้ายังไม่มี
await DbSeeder.SeedAsync(app.Services);

// ทุก response บอกเบราว์เซอร์ว่าห้ามเดาชนิดไฟล์เอง
app.Use(async (httpContext, next) =>
{
    httpContext.Response.Headers.XContentTypeOptions = "nosniff";
    await next();
});

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
