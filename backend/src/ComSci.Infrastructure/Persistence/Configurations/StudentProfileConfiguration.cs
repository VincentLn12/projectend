using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง StudentProfile (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        // กำหนดคีย์หลัก
        builder.HasKey(studentProfile => studentProfile.UserId);

        // จำกัดความยาว StudentCode ไม่เกิน 20 ตัวอักษร
        builder.Property(studentProfile => studentProfile.StudentCode)
            .HasMaxLength(20);

        // จำกัดความยาว PreviousSchool ไม่เกิน 200 ตัวอักษร
        builder.Property(studentProfile => studentProfile.PreviousSchool)
            .HasMaxLength(200);

        // จำกัดความยาว Address ไม่เกิน 500 ตัวอักษร
        builder.Property(studentProfile => studentProfile.Address)
            .HasMaxLength(500);

        // สร้าง index เพื่อให้ค้นหาเร็วขึ้น และห้ามค่าซ้ำ
        builder.HasIndex(studentProfile => studentProfile.StudentCode)
            .IsUnique();

        // ความสัมพันธ์: StudentProfile.User (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(studentProfile => studentProfile.User)
            .WithOne(user => user.StudentProfile)
            .HasForeignKey<StudentProfile>(studentProfile => studentProfile.UserId);
    }
}
