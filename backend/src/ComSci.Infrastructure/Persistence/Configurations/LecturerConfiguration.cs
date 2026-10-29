using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class LecturerConfiguration : IEntityTypeConfiguration<Lecturer>
{
    public void Configure(EntityTypeBuilder<Lecturer> builder)
    {
        builder.Property(lecturer => lecturer.Prefix)
            .HasMaxLength(50);

        builder.Property(lecturer => lecturer.FullName)
            .HasMaxLength(200);

        builder.Property(lecturer => lecturer.Position)
            .HasMaxLength(200);

        builder.Property(lecturer => lecturer.Expertise)
            .HasMaxLength(500);

        builder.Property(lecturer => lecturer.EducationLevel)
            .HasMaxLength(100);

        builder.Property(lecturer => lecturer.Program)
            .HasMaxLength(200);

        builder.Property(lecturer => lecturer.Contact)
            .HasMaxLength(300);

        builder.HasOne(lecturer => lecturer.User)
            .WithMany()
            .HasForeignKey(lecturer => lecturer.UserId);

        builder.HasOne(lecturer => lecturer.ImageFile)
            .WithMany()
            .HasForeignKey(lecturer => lecturer.ImageFileId);
    }
}
