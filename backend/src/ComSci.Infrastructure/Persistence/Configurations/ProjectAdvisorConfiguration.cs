using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class ProjectAdvisorConfiguration : IEntityTypeConfiguration<ProjectAdvisor>
{
    public void Configure(EntityTypeBuilder<ProjectAdvisor> builder)
    {
        builder.Property(projectAdvisor => projectAdvisor.Role)
            .HasConversion<byte>();

        builder.HasIndex(projectAdvisor => new { projectAdvisor.ProjectId, projectAdvisor.LecturerId })
            .IsUnique();

        builder.HasOne(projectAdvisor => projectAdvisor.Project)
            .WithMany(project => project.Advisors)
            .HasForeignKey(projectAdvisor => projectAdvisor.ProjectId);

        builder.HasOne(projectAdvisor => projectAdvisor.Lecturer)
            .WithMany()
            .HasForeignKey(projectAdvisor => projectAdvisor.LecturerId);
    }
}
