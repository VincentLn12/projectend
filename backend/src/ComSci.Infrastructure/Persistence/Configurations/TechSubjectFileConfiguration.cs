using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class TechSubjectFileConfiguration : IEntityTypeConfiguration<TechSubjectFile>
{
    public void Configure(EntityTypeBuilder<TechSubjectFile> builder)
    {
        builder.HasOne(techSubjectFile => techSubjectFile.TechSubject)
            .WithMany(techSubject => techSubject.Files)
            .HasForeignKey(techSubjectFile => techSubjectFile.TechSubjectId);

        builder.HasOne(techSubjectFile => techSubjectFile.File)
            .WithMany()
            .HasForeignKey(techSubjectFile => techSubjectFile.FileId);
    }
}
