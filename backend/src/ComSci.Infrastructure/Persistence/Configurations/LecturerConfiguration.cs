using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง Lecturer (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class LecturerConfiguration : IEntityTypeConfiguration<Lecturer>
{
    public void Configure(EntityTypeBuilder<Lecturer> builder)
    {
        // จำกัดความยาว Prefix ไม่เกิน 50 ตัวอักษร
        builder.Property(lecturer => lecturer.Prefix)
            .HasMaxLength(50);

        // จำกัดความยาว FullName ไม่เกิน 200 ตัวอักษร
        builder.Property(lecturer => lecturer.FullName)
            .HasMaxLength(200);

        // จำกัดความยาว Position ไม่เกิน 200 ตัวอักษร
        builder.Property(lecturer => lecturer.Position)
            .HasMaxLength(200);

        // จำกัดความยาว Expertise ไม่เกิน 500 ตัวอักษร
        builder.Property(lecturer => lecturer.Expertise)
            .HasMaxLength(500);

        // จำกัดความยาว EducationLevel ไม่เกิน 100 ตัวอักษร
        builder.Property(lecturer => lecturer.EducationLevel)
            .HasMaxLength(100);

        // จำกัดความยาว Program ไม่เกิน 200 ตัวอักษร
        builder.Property(lecturer => lecturer.Program)
            .HasMaxLength(200);

        // จำกัดความยาว Contact ไม่เกิน 300 ตัวอักษร
        builder.Property(lecturer => lecturer.Contact)
            .HasMaxLength(300);

        // ความสัมพันธ์: Lecturer.User (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(lecturer => lecturer.User)
            .WithMany()
            .HasForeignKey(lecturer => lecturer.UserId);

        // ความสัมพันธ์: Lecturer.ImageFile (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(lecturer => lecturer.ImageFile)
            .WithMany()
            .HasForeignKey(lecturer => lecturer.ImageFileId);
    }
}
