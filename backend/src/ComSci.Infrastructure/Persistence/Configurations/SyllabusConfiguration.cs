using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class SyllabusConfiguration : IEntityTypeConfiguration<Syllabus>
{
    public void Configure(EntityTypeBuilder<Syllabus> builder)
    {
        builder.Property(syllabus => syllabus.NameTH)
            .HasMaxLength(300);

        builder.Property(syllabus => syllabus.NameEN)
            .HasMaxLength(300);

        builder.Property(syllabus => syllabus.DegreeTH)
            .HasMaxLength(200);

        builder.Property(syllabus => syllabus.DegreeEN)
            .HasMaxLength(200);

        builder.HasOne(syllabus => syllabus.PdfFile)
            .WithMany()
            .HasForeignKey(syllabus => syllabus.PdfFileId);
    }
}
