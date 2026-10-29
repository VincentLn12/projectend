using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class CoursePhotoConfiguration : IEntityTypeConfiguration<CoursePhoto>
{
    public void Configure(EntityTypeBuilder<CoursePhoto> builder)
    {
        builder.HasOne(coursePhoto => coursePhoto.Course)
            .WithMany(course => course.Photos)
            .HasForeignKey(coursePhoto => coursePhoto.CourseId);

        builder.HasOne(coursePhoto => coursePhoto.File)
            .WithMany()
            .HasForeignKey(coursePhoto => coursePhoto.FileId);
    }
}
