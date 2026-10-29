using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class TechSubjectConfiguration : IEntityTypeConfiguration<TechSubject>
{
    public void Configure(EntityTypeBuilder<TechSubject> builder)
    {
        builder.Property(techSubject => techSubject.Icon)
            .HasMaxLength(100);

        builder.Property(techSubject => techSubject.SubjectName)
            .HasMaxLength(200);

        builder.Property(techSubject => techSubject.SubTitle)
            .HasMaxLength(300);
    }
}
