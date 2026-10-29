using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class SyllabusObjectiveConfiguration : IEntityTypeConfiguration<SyllabusObjective>
{
    public void Configure(EntityTypeBuilder<SyllabusObjective> builder)
    {
        builder.Property(syllabusObjective => syllabusObjective.Sentence)
            .HasMaxLength(500);

        builder.HasOne(syllabusObjective => syllabusObjective.Syllabus)
            .WithMany(syllabus => syllabus.Objectives)
            .HasForeignKey(syllabusObjective => syllabusObjective.SyllabusId);
    }
}
