using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class SyllabusCategoryFileConfiguration : IEntityTypeConfiguration<SyllabusCategoryFile>
{
    public void Configure(EntityTypeBuilder<SyllabusCategoryFile> builder)
    {
        builder.HasOne(categoryFile => categoryFile.Syllabus)
            .WithMany(syllabus => syllabus.CategoryFiles)
            .HasForeignKey(categoryFile => categoryFile.SyllabusId);

        builder.HasOne(categoryFile => categoryFile.File)
            .WithMany()
            .HasForeignKey(categoryFile => categoryFile.FileId);

        builder.HasIndex(categoryFile => new { categoryFile.SyllabusId, categoryFile.SubjectCategory })
            .IsUnique();
    }
}
