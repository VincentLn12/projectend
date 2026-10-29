using ComSci.Application.Constants;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComSci.Infrastructure.Persistence;

public static class DbSeeder
{
    /// <summary>Idempotent: creates the roles, the first admin and the settings row only when missing.</summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var log = sp.GetRequiredService<ILogger<AppDbContext>>();
        var config = sp.GetRequiredService<IConfiguration>();
        var roles = sp.GetRequiredService<RoleManager<AppRole>>();
        var users = sp.GetRequiredService<UserManager<AppUser>>();
        var db = sp.GetRequiredService<AppDbContext>();

        foreach (var name in Roles.All)
            if (!await roles.RoleExistsAsync(name))
                await roles.CreateAsync(new AppRole(name));

        var email = config["Seed:AdminEmail"];
        var password = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            log.LogWarning("Seed:AdminEmail / Seed:AdminPassword not set; no admin account was created.");
        }
        else if (await users.FindByEmailAsync(email) is null)
        {
            var admin = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "Administrator",
                UserType = UserType.Admin,
            };
            var created = await users.CreateAsync(admin, password);
            if (created.Succeeded) await users.AddToRoleAsync(admin, Roles.Admin);
            else log.LogError("Could not create admin: {Errors}", string.Join("; ", created.Errors.Select(e => e.Description)));
        }

        if (!await db.SystemSettings.AnyAsync())
        {
            db.SystemSettings.Add(new SystemSetting { WebName = "สาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยราชภัฏกาญจนบุรี" });
            await db.SaveChangesAsync();
        }
    }
}
