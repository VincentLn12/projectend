using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class ShortCourseConfiguration : IEntityTypeConfiguration<ShortCourse>
{
    public void Configure(EntityTypeBuilder<ShortCourse> builder)
    {
        builder.Property(shortCourse => shortCourse.Title)
            .HasMaxLength(300);

        builder.Property(shortCourse => shortCourse.Audience)
            .HasMaxLength(500);

        builder.Property(shortCourse => shortCourse.Location)
            .HasMaxLength(500);

        builder.Property(shortCourse => shortCourse.TeacherName)
            .HasMaxLength(300);

        builder.HasOne(shortCourse => shortCourse.CoverFile)
            .WithMany()
            .HasForeignKey(shortCourse => shortCourse.CoverFileId);

        builder.HasOne(shortCourse => shortCourse.CertificateTemplate)
            .WithMany()
            .HasForeignKey(shortCourse => shortCourse.CertificateTemplateId);
    }
}
