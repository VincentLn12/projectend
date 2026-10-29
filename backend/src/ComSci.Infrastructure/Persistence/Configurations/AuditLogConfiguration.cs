using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง AuditLog (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // จำกัดความยาว Action ไม่เกิน 20 ตัวอักษร
        builder.Property(auditLog => auditLog.Action)
            .HasMaxLength(20);

        // จำกัดความยาว EntityName ไม่เกิน 100 ตัวอักษร
        builder.Property(auditLog => auditLog.EntityName)
            .HasMaxLength(100);

        // จำกัดความยาว EntityId ไม่เกิน 50 ตัวอักษร
        builder.Property(auditLog => auditLog.EntityId)
            .HasMaxLength(50);

        // สร้าง index เพื่อให้ค้นหาเร็วขึ้น
        builder.HasIndex(auditLog => new { auditLog.EntityName, auditLog.EntityId });
    }
}
