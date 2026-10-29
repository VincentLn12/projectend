using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง Course (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        // จำกัดความยาว Title ไม่เกิน 300 ตัวอักษร
        builder.Property(course => course.Title)
            .HasMaxLength(300);

        // ความสัมพันธ์: Course.Lecturer (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(course => course.Lecturer)
            .WithMany()
            .HasForeignKey(course => course.LecturerId);
    }
}
