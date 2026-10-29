using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง Project (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // จำกัดความยาว NameTH ไม่เกิน 300 ตัวอักษร
        builder.Property(project => project.NameTH)
            .HasMaxLength(300);

        // จำกัดความยาว NameEN ไม่เกิน 300 ตัวอักษร
        builder.Property(project => project.NameEN)
            .HasMaxLength(300);

        // จำกัดความยาว KeyWords ไม่เกิน 500 ตัวอักษร
        builder.Property(project => project.KeyWords)
            .HasMaxLength(500);

        // จำกัดความยาว VideoUrl ไม่เกิน 500 ตัวอักษร
        builder.Property(project => project.VideoUrl)
            .HasMaxLength(500);

        // จำกัดความยาว WebUrl ไม่เกิน 500 ตัวอักษร
        builder.Property(project => project.WebUrl)
            .HasMaxLength(500);

        // จำกัดความยาว GithubUrl ไม่เกิน 500 ตัวอักษร
        builder.Property(project => project.GithubUrl)
            .HasMaxLength(500);

        // สร้าง index เพื่อให้ค้นหาเร็วขึ้น
        builder.HasIndex(project => new { project.AcademicYear, project.IsPublished, project.IsDeleted });

        // ความสัมพันธ์: Project.Student (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(project => project.Student)
            .WithMany()
            .HasForeignKey(project => project.StudentId);

        // ความสัมพันธ์: Project.ImageFile (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(project => project.ImageFile)
            .WithMany()
            .HasForeignKey(project => project.ImageFileId);

        // ความสัมพันธ์: Project.PdfFile (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(project => project.PdfFile)
            .WithMany()
            .HasForeignKey(project => project.PdfFileId);
    }
}
