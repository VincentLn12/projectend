using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class StudentProjectConfiguration : IEntityTypeConfiguration<StudentProject>
{
    public void Configure(EntityTypeBuilder<StudentProject> builder)
    {
        builder.Property(project => project.Title)
            .HasMaxLength(300);

        builder.Property(project => project.CourseName)
            .HasMaxLength(200);

        builder.Property(project => project.Technologies)
            .HasMaxLength(500);

        builder.Property(project => project.VideoUrl)
            .HasMaxLength(500);

        builder.Property(project => project.WebUrl)
            .HasMaxLength(500);

        builder.Property(project => project.GithubUrl)
            .HasMaxLength(500);

        builder.HasIndex(project => new { project.StudentId, project.AcademicYear, project.Semester });

        builder.HasIndex(project => new { project.AcademicYear, project.Semester, project.IsPublished, project.IsDeleted });

        builder.HasOne(project => project.Student)
            .WithMany()
            .HasForeignKey(project => project.StudentId);

        builder.HasOne(project => project.Lecturer)
            .WithMany()
            .HasForeignKey(project => project.LecturerId);

        builder.HasOne(project => project.ImageFile)
            .WithMany()
            .HasForeignKey(project => project.ImageFileId);

        builder.HasOne(project => project.PdfFile)
            .WithMany()
            .HasForeignKey(project => project.PdfFileId);
    }
}
