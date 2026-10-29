using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง CoursePhoto (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class CoursePhotoConfiguration : IEntityTypeConfiguration<CoursePhoto>
{
    public void Configure(EntityTypeBuilder<CoursePhoto> builder)
    {
        // ความสัมพันธ์: CoursePhoto.Course (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(coursePhoto => coursePhoto.Course)
            .WithMany(course => course.Photos)
            .HasForeignKey(coursePhoto => coursePhoto.CourseId);

        // ความสัมพันธ์: CoursePhoto.File (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(coursePhoto => coursePhoto.File)
            .WithMany()
            .HasForeignKey(coursePhoto => coursePhoto.FileId);
    }
}
