using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class FileRecordConfiguration : IEntityTypeConfiguration<FileRecord>
{
    public void Configure(EntityTypeBuilder<FileRecord> builder)
    {
        builder.ToTable("Files");

        builder.Property(fileRecord => fileRecord.OriginalName)
            .HasMaxLength(260);

        builder.Property(fileRecord => fileRecord.StoragePath)
            .HasMaxLength(500);

        builder.Property(fileRecord => fileRecord.ContentType)
            .HasMaxLength(100);
    }
}
