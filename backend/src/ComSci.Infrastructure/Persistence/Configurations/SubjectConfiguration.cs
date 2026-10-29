using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.Property(subject => subject.Code)
            .HasMaxLength(20);

        builder.Property(subject => subject.Name)
            .HasMaxLength(200);

        builder.Property(subject => subject.Credit)
            .HasPrecision(3, 1);

        builder.HasOne(subject => subject.Syllabus)
            .WithMany(syllabus => syllabus.Subjects)
            .HasForeignKey(subject => subject.SyllabusId);
    }
}
