using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class JobHistoryConfiguration : IEntityTypeConfiguration<JobHistory>
{
    public void Configure(EntityTypeBuilder<JobHistory> builder)
    {
        builder.Property(jobHistory => jobHistory.JobName)
            .HasMaxLength(200);

        builder.Property(jobHistory => jobHistory.Company)
            .HasMaxLength(200);

        builder.Property(jobHistory => jobHistory.Position)
            .HasMaxLength(200);

        builder.Property(jobHistory => jobHistory.Description)
            .HasMaxLength(1000);

        builder.HasOne(jobHistory => jobHistory.User)
            .WithMany(user => user.JobHistories)
            .HasForeignKey(jobHistory => jobHistory.UserId);
    }
}
