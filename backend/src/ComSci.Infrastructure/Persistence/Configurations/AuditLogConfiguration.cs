using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(auditLog => auditLog.Action)
            .HasMaxLength(20);

        builder.Property(auditLog => auditLog.EntityName)
            .HasMaxLength(100);

        builder.Property(auditLog => auditLog.EntityId)
            .HasMaxLength(50);

        builder.HasIndex(auditLog => new { auditLog.EntityName, auditLog.EntityId });
    }
}
