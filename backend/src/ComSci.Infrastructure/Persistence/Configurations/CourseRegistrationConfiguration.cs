using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง CourseRegistration (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class CourseRegistrationConfiguration : IEntityTypeConfiguration<CourseRegistration>
{
    public void Configure(EntityTypeBuilder<CourseRegistration> builder)
    {
        // กำหนดคีย์หลัก
        builder.HasKey(courseRegistration => new { courseRegistration.UserId, courseRegistration.GenerationId });

        // เก็บ Status ในฐานข้อมูลเป็นตัวเลข (byte)
        builder.Property(courseRegistration => courseRegistration.Status)
            .HasConversion<byte>();

        // ความสัมพันธ์: CourseRegistration.User (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(courseRegistration => courseRegistration.User)
            .WithMany()
            .HasForeignKey(courseRegistration => courseRegistration.UserId);

        // ความสัมพันธ์: CourseRegistration.Generation (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(courseRegistration => courseRegistration.Generation)
            .WithMany(generation => generation.Registrations)
            .HasForeignKey(courseRegistration => courseRegistration.GenerationId);
    }
}
