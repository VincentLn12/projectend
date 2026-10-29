using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง TechSubjectFile (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class TechSubjectFileConfiguration : IEntityTypeConfiguration<TechSubjectFile>
{
    public void Configure(EntityTypeBuilder<TechSubjectFile> builder)
    {
        // ความสัมพันธ์: TechSubjectFile.TechSubject (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(techSubjectFile => techSubjectFile.TechSubject)
            .WithMany(techSubject => techSubject.Files)
            .HasForeignKey(techSubjectFile => techSubjectFile.TechSubjectId);

        // ความสัมพันธ์: TechSubjectFile.File (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(techSubjectFile => techSubjectFile.File)
            .WithMany()
            .HasForeignKey(techSubjectFile => techSubjectFile.FileId);
    }
}
