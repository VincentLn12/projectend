using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class CertificateTemplateConfiguration : IEntityTypeConfiguration<CertificateTemplate>
{
    public void Configure(EntityTypeBuilder<CertificateTemplate> builder)
    {
        builder.Property(template => template.Name)
            .HasMaxLength(200);

        builder.Property(template => template.SignerName)
            .HasMaxLength(200);

        builder.Property(template => template.SignerPosition)
            .HasMaxLength(300);

        builder.Property(template => template.SecondSignerName)
            .HasMaxLength(200);

        builder.Property(template => template.SecondSignerPosition)
            .HasMaxLength(300);

        builder.HasOne(template => template.BackgroundFile)
            .WithMany()
            .HasForeignKey(template => template.BackgroundFileId);
    }
}
