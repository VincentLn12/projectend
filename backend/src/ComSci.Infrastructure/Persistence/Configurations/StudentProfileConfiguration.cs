using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.HasKey(studentProfile => studentProfile.UserId);

        builder.Property(studentProfile => studentProfile.StudentCode)
            .HasMaxLength(20);

        builder.Property(studentProfile => studentProfile.PreviousSchool)
            .HasMaxLength(200);

        builder.Property(studentProfile => studentProfile.Address)
            .HasMaxLength(500);

        builder.HasIndex(studentProfile => studentProfile.StudentCode)
            .IsUnique();

        builder.HasOne(studentProfile => studentProfile.User)
            .WithOne(user => user.StudentProfile)
            .HasForeignKey<StudentProfile>(studentProfile => studentProfile.UserId);
    }
}
