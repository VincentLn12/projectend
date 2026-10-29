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
    public static async Task SeedAsync(IServiceProvider services)
    {
        using (IServiceScope scope = services.CreateScope())
        {
            IServiceProvider scopedServices = scope.ServiceProvider;

            RoleManager<AppRole> roleManager = scopedServices.GetRequiredService<RoleManager<AppRole>>();
            UserManager<AppUser> userManager = scopedServices.GetRequiredService<UserManager<AppUser>>();
            AppDbContext dbContext = scopedServices.GetRequiredService<AppDbContext>();
            IConfiguration configuration = scopedServices.GetRequiredService<IConfiguration>();
            ILogger<AppDbContext> logger = scopedServices.GetRequiredService<ILogger<AppDbContext>>();

            await SeedRolesAsync(roleManager);
            await SeedAdminAsync(userManager, configuration, logger);
            await SeedSystemSettingAsync(dbContext);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        foreach (string roleName in Roles.All)
        {
            bool exists = await roleManager.RoleExistsAsync(roleName);
            if (!exists)
            {
                await roleManager.CreateAsync(new AppRole(roleName));
            }
        }
    }

    private static async Task SeedAdminAsync(
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        ILogger<AppDbContext> logger)
    {
        // อีเมลและรหัสผ่านแอดมินอยู่ใน user-secrets ไม่ใส่ในโค้ด
        string? email = configuration["Seed:AdminEmail"];
        string? password = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:AdminEmail / Seed:AdminPassword not set; no admin account was created.");
            return;
        }

        AppUser? existingAdmin = await userManager.FindByEmailAsync(email);
        if (existingAdmin != null)
        {
            return;
        }

        AppUser admin = new AppUser();
        admin.UserName = email;
        admin.Email = email;
        admin.EmailConfirmed = true;
        admin.FullName = "Administrator";
        admin.UserType = UserType.Admin;

        IdentityResult createResult = await userManager.CreateAsync(admin, password);
        if (!createResult.Succeeded)
        {
            List<string> errors = createResult.Errors.Select(error => error.Description).ToList();
            logger.LogError("Could not create admin: {Errors}", string.Join("; ", errors));
            return;
        }

        await userManager.AddToRoleAsync(admin, Roles.Admin);
    }

    private static async Task SeedSystemSettingAsync(AppDbContext dbContext)
    {
        bool hasSetting = await dbContext.SystemSettings.AnyAsync();
        if (hasSetting)
        {
            return;
        }

        SystemSetting setting = new SystemSetting();
        setting.WebName = "สาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยราชภัฏกาญจนบุรี";

        dbContext.SystemSettings.Add(setting);
        await dbContext.SaveChangesAsync();
    }
}
