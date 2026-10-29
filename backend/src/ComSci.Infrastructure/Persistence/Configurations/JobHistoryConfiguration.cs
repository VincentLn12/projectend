using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง JobHistory (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class JobHistoryConfiguration : IEntityTypeConfiguration<JobHistory>
{
    public void Configure(EntityTypeBuilder<JobHistory> builder)
    {
        // จำกัดความยาว JobName ไม่เกิน 200 ตัวอักษร
        builder.Property(jobHistory => jobHistory.JobName)
            .HasMaxLength(200);

        // จำกัดความยาว Company ไม่เกิน 200 ตัวอักษร
        builder.Property(jobHistory => jobHistory.Company)
            .HasMaxLength(200);

        // จำกัดความยาว Position ไม่เกิน 200 ตัวอักษร
        builder.Property(jobHistory => jobHistory.Position)
            .HasMaxLength(200);

        // จำกัดความยาว Description ไม่เกิน 1000 ตัวอักษร
        builder.Property(jobHistory => jobHistory.Description)
            .HasMaxLength(1000);

        // ความสัมพันธ์: JobHistory.User (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(jobHistory => jobHistory.User)
            .WithMany(user => user.JobHistories)
            .HasForeignKey(jobHistory => jobHistory.UserId);
    }
}
