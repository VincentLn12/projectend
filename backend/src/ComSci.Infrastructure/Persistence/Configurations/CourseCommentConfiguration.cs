using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง CourseComment (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class CourseCommentConfiguration : IEntityTypeConfiguration<CourseComment>
{
    public void Configure(EntityTypeBuilder<CourseComment> builder)
    {
        // จำกัดความยาว Body ไม่เกิน 1000 ตัวอักษร
        builder.Property(courseComment => courseComment.Body)
            .HasMaxLength(1000);

        // ความสัมพันธ์: CourseComment.Author (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(courseComment => courseComment.Author)
            .WithMany()
            .HasForeignKey(courseComment => courseComment.AuthorId);

        // ความสัมพันธ์: CourseComment.Generation (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(courseComment => courseComment.Generation)
            .WithMany(generation => generation.Comments)
            .HasForeignKey(courseComment => courseComment.GenerationId);
    }
}
