using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง SyllabusOccupation (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class SyllabusOccupationConfiguration : IEntityTypeConfiguration<SyllabusOccupation>
{
    public void Configure(EntityTypeBuilder<SyllabusOccupation> builder)
    {
        // จำกัดความยาว Sentence ไม่เกิน 500 ตัวอักษร
        builder.Property(syllabusOccupation => syllabusOccupation.Sentence)
            .HasMaxLength(500);

        // ความสัมพันธ์: SyllabusOccupation.Syllabus (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(syllabusOccupation => syllabusOccupation.Syllabus)
            .WithMany(syllabus => syllabus.Occupations)
            .HasForeignKey(syllabusOccupation => syllabusOccupation.SyllabusId);
    }
}
