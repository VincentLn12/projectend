using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง ProjectAdvisor (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class ProjectAdvisorConfiguration : IEntityTypeConfiguration<ProjectAdvisor>
{
    public void Configure(EntityTypeBuilder<ProjectAdvisor> builder)
    {
        // เก็บ Role ในฐานข้อมูลเป็นตัวเลข (byte)
        builder.Property(projectAdvisor => projectAdvisor.Role)
            .HasConversion<byte>();

        // สร้าง index เพื่อให้ค้นหาเร็วขึ้น และห้ามค่าซ้ำ
        builder.HasIndex(projectAdvisor => new { projectAdvisor.ProjectId, projectAdvisor.LecturerId })
            .IsUnique();

        // ความสัมพันธ์: ProjectAdvisor.Project (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(projectAdvisor => projectAdvisor.Project)
            .WithMany(project => project.Advisors)
            .HasForeignKey(projectAdvisor => projectAdvisor.ProjectId);

        // ความสัมพันธ์: ProjectAdvisor.Lecturer (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(projectAdvisor => projectAdvisor.Lecturer)
            .WithMany()
            .HasForeignKey(projectAdvisor => projectAdvisor.LecturerId);
    }
}
