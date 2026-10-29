using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง AppUser (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        // จำกัดความยาว FullName ไม่เกิน 200 ตัวอักษร
        builder.Property(appUser => appUser.FullName)
            .HasMaxLength(200);

        // จำกัดความยาว Bio ไม่เกิน 1000 ตัวอักษร
        builder.Property(appUser => appUser.Bio)
            .HasMaxLength(1000);

        // เก็บ UserType ในฐานข้อมูลเป็นตัวเลข (byte)
        builder.Property(appUser => appUser.UserType)
            .HasConversion<byte>();

        // สร้าง index เพื่อให้ค้นหาเร็วขึ้น และห้ามค่าซ้ำ
        builder.HasIndex(appUser => appUser.NormalizedEmail)
            .IsUnique()
            .HasFilter("[NormalizedEmail] IS NOT NULL")
            .HasDatabaseName("IX_Users_Email");

        // ความสัมพันธ์: AppUser.ImageFile (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(appUser => appUser.ImageFile)
            .WithMany()
            .HasForeignKey(appUser => appUser.ImageFileId);
    }
}
