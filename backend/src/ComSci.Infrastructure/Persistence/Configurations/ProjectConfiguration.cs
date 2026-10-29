using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(project => project.NameTH)
            .HasMaxLength(300);

        builder.Property(project => project.NameEN)
            .HasMaxLength(300);

        builder.Property(project => project.KeyWords)
            .HasMaxLength(500);

        builder.Property(project => project.VideoUrl)
            .HasMaxLength(500);

        builder.Property(project => project.WebUrl)
            .HasMaxLength(500);

        builder.Property(project => project.GithubUrl)
            .HasMaxLength(500);

        builder.HasIndex(project => new { project.AcademicYear, project.IsPublished, project.IsDeleted });

        builder.HasOne(project => project.Student)
            .WithMany()
            .HasForeignKey(project => project.StudentId);

        builder.HasOne(project => project.ImageFile)
            .WithMany()
            .HasForeignKey(project => project.ImageFileId);

        builder.HasOne(project => project.PdfFile)
            .WithMany()
            .HasForeignKey(project => project.PdfFileId);
    }
}
