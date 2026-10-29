using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง Generation (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class GenerationConfiguration : IEntityTypeConfiguration<Generation>
{
    public void Configure(EntityTypeBuilder<Generation> builder)
    {
        // จำกัดความยาว SubTitle ไม่เกิน 300 ตัวอักษร
        builder.Property(generation => generation.SubTitle)
            .HasMaxLength(300);

        // Fee: ตัวเลขรวม 10 หลัก ทศนิยม 2 หลัก
        builder.Property(generation => generation.Fee)
            .HasPrecision(10, 2);

        // ความสัมพันธ์: Generation.Course (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(generation => generation.Course)
            .WithMany(course => course.Generations)
            .HasForeignKey(generation => generation.CourseId);

        // ความสัมพันธ์: Generation.PhotoFile (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(generation => generation.PhotoFile)
            .WithMany()
            .HasForeignKey(generation => generation.PhotoFileId);
    }
}
