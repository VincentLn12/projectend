using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง Subject (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        // จำกัดความยาว Code ไม่เกิน 20 ตัวอักษร
        builder.Property(subject => subject.Code)
            .HasMaxLength(20);

        // จำกัดความยาว Name ไม่เกิน 200 ตัวอักษร
        builder.Property(subject => subject.Name)
            .HasMaxLength(200);

        // Credit: ตัวเลขรวม 3 หลัก ทศนิยม 1 หลัก
        builder.Property(subject => subject.Credit)
            .HasPrecision(3, 1);

        // ความสัมพันธ์: Subject.Syllabus (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(subject => subject.Syllabus)
            .WithMany(syllabus => syllabus.Subjects)
            .HasForeignKey(subject => subject.SyllabusId);
    }
}
