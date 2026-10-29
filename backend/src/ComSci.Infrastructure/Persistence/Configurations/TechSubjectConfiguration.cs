using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง TechSubject (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class TechSubjectConfiguration : IEntityTypeConfiguration<TechSubject>
{
    public void Configure(EntityTypeBuilder<TechSubject> builder)
    {
        // จำกัดความยาว Icon ไม่เกิน 100 ตัวอักษร
        builder.Property(techSubject => techSubject.Icon)
            .HasMaxLength(100);

        // จำกัดความยาว SubjectName ไม่เกิน 200 ตัวอักษร
        builder.Property(techSubject => techSubject.SubjectName)
            .HasMaxLength(200);

        // จำกัดความยาว SubTitle ไม่เกิน 300 ตัวอักษร
        builder.Property(techSubject => techSubject.SubTitle)
            .HasMaxLength(300);
    }
}
