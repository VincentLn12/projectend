using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.Property(course => course.Title)
            .HasMaxLength(300);

        builder.HasOne(course => course.Lecturer)
            .WithMany()
            .HasForeignKey(course => course.LecturerId);
    }
}
