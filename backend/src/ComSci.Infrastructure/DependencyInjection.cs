using ComSci.Application.Abstractions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Files;
using ComSci.Infrastructure.Identity;
using ComSci.Infrastructure.Persistence;
using ComSci.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComSci.Infrastructure;

/// <summary>
/// ลงทะเบียน service ทั้งหมดของชั้น Infrastructure เข้ากับระบบ dependency injection
/// เรียกจาก Program.cs ด้วย services.AddInfrastructure(configuration)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddIdentity(services);
        AddApplicationServices(services);

        return services;
    }

    /// <summary>
    /// ฐานข้อมูล SQL Server พร้อม interceptor ที่เติม audit field และทำ soft delete
    /// </summary>
    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            string? connectionString = configuration.GetConnectionString("Default");
            options.UseSqlServer(connectionString);

            // ตารางเชื่อมของ Identity (UserRoles, UserClaims ฯลฯ) ไม่มี navigation จึงใส่ query filter คู่กับ Users ไม่ได้
            // EF จะเตือนเรื่องนี้ ซึ่งในกรณีนี้ไม่มีผลเสีย จึงปิดคำเตือนไว้
            options.ConfigureWarnings(warnings =>
            {
                warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
            });

            AuditSaveChangesInterceptor interceptor = serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>();
            options.AddInterceptors(interceptor);
        });
    }

    /// <summary>
    /// ASP.NET Identity: ผู้ใช้ บทบาท การเข้ารหัสรหัสผ่าน และการล็อกบัญชี
    /// </summary>
    private static void AddIdentity(IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;

                // ใส่รหัสผ่านผิดครบ 5 ครั้ง ล็อกบัญชี 15 นาที
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>();
    }

    /// <summary>
    /// service ของระบบ (ผูก interface ในชั้น Application เข้ากับคลาสจริง)
    /// </summary>
    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IFileService, FileService>();

        // ที่เก็บไฟล์ไม่เก็บสถานะอะไร จึงใช้ตัวเดียวทั้งแอปได้
        services.AddSingleton<IFileStorage, LocalFileStorage>();
    }
}
