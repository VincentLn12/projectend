using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class GenerationConfiguration : IEntityTypeConfiguration<Generation>
{
    public void Configure(EntityTypeBuilder<Generation> builder)
    {
        builder.Property(generation => generation.SubTitle)
            .HasMaxLength(300);

        builder.Property(generation => generation.Fee)
            .HasPrecision(10, 2);

        builder.HasOne(generation => generation.Course)
            .WithMany(course => course.Generations)
            .HasForeignKey(generation => generation.CourseId);

        builder.HasOne(generation => generation.PhotoFile)
            .WithMany()
            .HasForeignKey(generation => generation.PhotoFileId);
    }
}
