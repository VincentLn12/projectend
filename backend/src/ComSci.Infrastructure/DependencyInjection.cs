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

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddIdentity(services);
        AddApplicationServices(services);

        return services;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            string? connectionString = configuration.GetConnectionString("Default");
            options.UseSqlServer(connectionString);

            // ตารางเชื่อมของ Identity ไม่มี navigation จึงใส่ query filter คู่กับ Users ไม่ได้ ปิดคำเตือนนี้
            options.ConfigureWarnings(warnings =>
            {
                warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
            });

            AuditSaveChangesInterceptor interceptor = serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>();
            options.AddInterceptors(interceptor);
        });
    }

    private static void AddIdentity(IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ILecturerService, LecturerService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IStudentProjectService, StudentProjectService>();
        services.AddScoped<IMyProjectService, MyProjectService>();
        services.AddScoped<IMyJobHistoryService, MyJobHistoryService>();
        services.AddScoped<IShortCourseService, ShortCourseService>();
        services.AddScoped<IShortCourseRegistrationService, ShortCourseRegistrationService>();
        services.AddScoped<IShortCourseDocumentService, ShortCourseDocumentService>();
        services.AddScoped<ICertificateTemplateService, CertificateTemplateService>();
        services.AddScoped<IHomeBannerService, HomeBannerService>();
        services.AddScoped<ITechSubjectService, TechSubjectService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IJobHistoryService, JobHistoryService>();
        services.AddScoped<ISyllabusService, SyllabusService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IUserAdminService, UserAdminService>();

        services.AddSingleton<IFileStorage, LocalFileStorage>();
    }
}
