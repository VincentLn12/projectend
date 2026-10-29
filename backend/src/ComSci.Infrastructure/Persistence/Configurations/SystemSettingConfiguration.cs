using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง SystemSetting (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        // จำกัดความยาว WebName ไม่เกิน 200 ตัวอักษร
        builder.Property(systemSetting => systemSetting.WebName)
            .HasMaxLength(200);

        // จำกัดความยาว RegisterUrl ไม่เกิน 500 ตัวอักษร
        builder.Property(systemSetting => systemSetting.RegisterUrl)
            .HasMaxLength(500);

        // จำกัดความยาว KruUrl ไม่เกิน 500 ตัวอักษร
        builder.Property(systemSetting => systemSetting.KruUrl)
            .HasMaxLength(500);

        // จำกัดความยาว PageFacebook ไม่เกิน 500 ตัวอักษร
        builder.Property(systemSetting => systemSetting.PageFacebook)
            .HasMaxLength(500);

        // จำกัดความยาว VideoUrl ไม่เกิน 500 ตัวอักษร
        builder.Property(systemSetting => systemSetting.VideoUrl)
            .HasMaxLength(500);

        // จำกัดความยาว MapLocation ไม่เกิน 500 ตัวอักษร
        builder.Property(systemSetting => systemSetting.MapLocation)
            .HasMaxLength(500);

        // ความสัมพันธ์: SystemSetting.LogoFile (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(systemSetting => systemSetting.LogoFile)
            .WithMany()
            .HasForeignKey(systemSetting => systemSetting.LogoFileId);
    }
}
