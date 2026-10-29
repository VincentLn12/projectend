using ComSci.Application.Abstractions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Identity;
using ComSci.Infrastructure.Services;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComSci.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INewsService, NewsService>();

        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseSqlServer(config.GetConnectionString("Default"))
            // Identity's join tables (UserRoles, UserClaims, ...) have no navigation to hang a matching filter on.
            .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning))
            .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddIdentityCore<AppUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.Password.RequiredLength = 8;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services;
    }
}

