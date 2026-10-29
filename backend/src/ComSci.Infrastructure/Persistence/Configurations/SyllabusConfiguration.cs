using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง Syllabus (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class SyllabusConfiguration : IEntityTypeConfiguration<Syllabus>
{
    public void Configure(EntityTypeBuilder<Syllabus> builder)
    {
        // จำกัดความยาว NameTH ไม่เกิน 300 ตัวอักษร
        builder.Property(syllabus => syllabus.NameTH)
            .HasMaxLength(300);

        // จำกัดความยาว NameEN ไม่เกิน 300 ตัวอักษร
        builder.Property(syllabus => syllabus.NameEN)
            .HasMaxLength(300);

        // จำกัดความยาว DegreeTH ไม่เกิน 200 ตัวอักษร
        builder.Property(syllabus => syllabus.DegreeTH)
            .HasMaxLength(200);

        // จำกัดความยาว DegreeEN ไม่เกิน 200 ตัวอักษร
        builder.Property(syllabus => syllabus.DegreeEN)
            .HasMaxLength(200);

        // ความสัมพันธ์: Syllabus.PdfFile (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(syllabus => syllabus.PdfFile)
            .WithMany()
            .HasForeignKey(syllabus => syllabus.PdfFileId);
    }
}
