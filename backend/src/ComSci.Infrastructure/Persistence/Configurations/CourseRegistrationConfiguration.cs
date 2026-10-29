using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class CourseRegistrationConfiguration : IEntityTypeConfiguration<CourseRegistration>
{
    public void Configure(EntityTypeBuilder<CourseRegistration> builder)
    {
        builder.HasKey(courseRegistration => new { courseRegistration.UserId, courseRegistration.GenerationId });

        builder.Property(courseRegistration => courseRegistration.Status)
            .HasConversion<byte>();

        builder.HasOne(courseRegistration => courseRegistration.User)
            .WithMany()
            .HasForeignKey(courseRegistration => courseRegistration.UserId);

        builder.HasOne(courseRegistration => courseRegistration.Generation)
            .WithMany(generation => generation.Registrations)
            .HasForeignKey(courseRegistration => courseRegistration.GenerationId);
    }
}
