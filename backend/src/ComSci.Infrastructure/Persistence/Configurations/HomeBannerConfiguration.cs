using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง HomeBanner (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class HomeBannerConfiguration : IEntityTypeConfiguration<HomeBanner>
{
    public void Configure(EntityTypeBuilder<HomeBanner> builder)
    {
        // จำกัดความยาว Title ไม่เกิน 200 ตัวอักษร
        builder.Property(homeBanner => homeBanner.Title)
            .HasMaxLength(200);

        // จำกัดความยาว LinkUrl ไม่เกิน 500 ตัวอักษร
        builder.Property(homeBanner => homeBanner.LinkUrl)
            .HasMaxLength(500);

        // ความสัมพันธ์: HomeBanner.File (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(homeBanner => homeBanner.File)
            .WithMany()
            .HasForeignKey(homeBanner => homeBanner.FileId);
    }
}
