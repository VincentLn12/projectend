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

        builder.Property(systemSetting => systemSetting.MapLocation)
            .HasMaxLength(500);

        builder.HasOne(systemSetting => systemSetting.LogoFile)
            .WithMany()
            .HasForeignKey(systemSetting => systemSetting.LogoFileId);
    }
}
