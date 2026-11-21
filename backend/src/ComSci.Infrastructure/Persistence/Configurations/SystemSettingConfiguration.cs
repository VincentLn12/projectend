using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.Property(systemSetting => systemSetting.WebName)
            .HasMaxLength(200);

        builder.Property(systemSetting => systemSetting.RegisterUrl)
            .HasMaxLength(500);

        builder.Property(systemSetting => systemSetting.KruUrl)
            .HasMaxLength(500);

        builder.Property(systemSetting => systemSetting.PageFacebook)
            .HasMaxLength(500);

        builder.Property(systemSetting => systemSetting.VideoUrl)
            .HasMaxLength(500);

        builder.Property(systemSetting => systemSetting.UniversityName)
            .HasMaxLength(200);

        builder.Property(systemSetting => systemSetting.FooterText)
            .HasMaxLength(500);

        builder.Property(systemSetting => systemSetting.MapLocation)
            .HasMaxLength(500);

        builder.Property(systemSetting => systemSetting.AboutTitle)
            .HasMaxLength(300);

        builder.Property(systemSetting => systemSetting.AboutDescription)
            .HasMaxLength(2000);

        builder.Property(systemSetting => systemSetting.QuickMenuEnabled)
            .HasDefaultValue(true);

        builder.Property(systemSetting => systemSetting.QuickMenuItems)
            .HasMaxLength(2000);

        builder.HasOne(systemSetting => systemSetting.QuickMenuBackgroundFile)
            .WithMany()
            .HasForeignKey(systemSetting => systemSetting.QuickMenuBackgroundFileId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(systemSetting => systemSetting.LogoFile)
            .WithMany()
            .HasForeignKey(systemSetting => systemSetting.LogoFileId);
    }
}
