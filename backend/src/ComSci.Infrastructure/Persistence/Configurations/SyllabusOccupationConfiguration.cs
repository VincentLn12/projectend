using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class SyllabusOccupationConfiguration : IEntityTypeConfiguration<SyllabusOccupation>
{
    public void Configure(EntityTypeBuilder<SyllabusOccupation> builder)
    {
        builder.Property(syllabusOccupation => syllabusOccupation.Sentence)
            .HasMaxLength(500);

        builder.HasOne(syllabusOccupation => syllabusOccupation.Syllabus)
            .WithMany(syllabus => syllabus.Occupations)
            .HasForeignKey(syllabusOccupation => syllabusOccupation.SyllabusId);
    }
}
