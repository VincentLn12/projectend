using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง FileRecord (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class FileRecordConfiguration : IEntityTypeConfiguration<FileRecord>
{
    public void Configure(EntityTypeBuilder<FileRecord> builder)
    {
        // ใช้ชื่อตารางในฐานข้อมูลว่า Files
        builder.ToTable("Files");

        // จำกัดความยาว OriginalName ไม่เกิน 260 ตัวอักษร
        builder.Property(fileRecord => fileRecord.OriginalName)
            .HasMaxLength(260);

        // จำกัดความยาว StoragePath ไม่เกิน 500 ตัวอักษร
        builder.Property(fileRecord => fileRecord.StoragePath)
            .HasMaxLength(500);

        // จำกัดความยาว ContentType ไม่เกิน 100 ตัวอักษร
        builder.Property(fileRecord => fileRecord.ContentType)
            .HasMaxLength(100);
    }
}
