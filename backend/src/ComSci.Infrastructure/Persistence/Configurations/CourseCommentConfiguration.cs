using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class CourseCommentConfiguration : IEntityTypeConfiguration<CourseComment>
{
    public void Configure(EntityTypeBuilder<CourseComment> builder)
    {
        builder.Property(courseComment => courseComment.Body)
            .HasMaxLength(1000);

        builder.HasOne(courseComment => courseComment.Author)
            .WithMany()
            .HasForeignKey(courseComment => courseComment.AuthorId);

        builder.HasOne(courseComment => courseComment.Generation)
            .WithMany(generation => generation.Comments)
            .HasForeignKey(courseComment => courseComment.GenerationId);
    }
}
