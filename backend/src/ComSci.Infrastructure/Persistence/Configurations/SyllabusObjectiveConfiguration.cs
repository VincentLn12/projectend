using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง SyllabusObjective (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class SyllabusObjectiveConfiguration : IEntityTypeConfiguration<SyllabusObjective>
{
    public void Configure(EntityTypeBuilder<SyllabusObjective> builder)
    {
        // จำกัดความยาว Sentence ไม่เกิน 500 ตัวอักษร
        builder.Property(syllabusObjective => syllabusObjective.Sentence)
            .HasMaxLength(500);

        // ความสัมพันธ์: SyllabusObjective.Syllabus (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(syllabusObjective => syllabusObjective.Syllabus)
            .WithMany(syllabus => syllabus.Objectives)
            .HasForeignKey(syllabusObjective => syllabusObjective.SyllabusId);
    }
}
