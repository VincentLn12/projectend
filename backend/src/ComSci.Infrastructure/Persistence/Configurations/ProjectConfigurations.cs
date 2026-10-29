using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.Property(x => x.NameTH).HasMaxLength(300);
        b.Property(x => x.NameEN).HasMaxLength(300);
        b.Property(x => x.KeyWords).HasMaxLength(500);
        b.Property(x => x.VideoUrl).HasMaxLength(500);
        b.Property(x => x.WebUrl).HasMaxLength(500);
        b.Property(x => x.GithubUrl).HasMaxLength(500);
        b.HasIndex(x => new { x.AcademicYear, x.IsPublished, x.IsDeleted });
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasOne(x => x.ImageFile).WithMany().HasForeignKey(x => x.ImageFileId);
        b.HasOne(x => x.PdfFile).WithMany().HasForeignKey(x => x.PdfFileId);
    }
}

public class ProjectAdvisorConfiguration : IEntityTypeConfiguration<ProjectAdvisor>
{
    public void Configure(EntityTypeBuilder<ProjectAdvisor> b)
    {
        b.Property(x => x.Role).HasConversion<byte>();
        b.HasIndex(x => new { x.ProjectId, x.LecturerId }).IsUnique();
        b.HasOne(x => x.Project).WithMany(p => p.Advisors).HasForeignKey(x => x.ProjectId);
        b.HasOne(x => x.Lecturer).WithMany().HasForeignKey(x => x.LecturerId);
    }
}

public class JobHistoryConfiguration : IEntityTypeConfiguration<JobHistory>
{
    public void Configure(EntityTypeBuilder<JobHistory> b)
    {
        b.Property(x => x.JobName).HasMaxLength(200);
        b.Property(x => x.Company).HasMaxLength(200);
        b.Property(x => x.Position).HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasOne(x => x.User).WithMany(u => u.JobHistories).HasForeignKey(x => x.UserId);
    }
}
