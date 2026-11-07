using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(appUser => appUser.FullName)
            .HasMaxLength(200);

        builder.Property(appUser => appUser.Bio)
            .HasMaxLength(1000);

        builder.Property(appUser => appUser.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(appUser => appUser.GithubUrl)
            .HasMaxLength(500);

        builder.Property(appUser => appUser.Skills)
            .HasMaxLength(500);

        builder.Property(appUser => appUser.UserType)
            .HasConversion<byte>();

        builder.HasIndex(appUser => appUser.NormalizedEmail)
            .IsUnique()
            .HasFilter("[NormalizedEmail] IS NOT NULL")
            .HasDatabaseName("IX_Users_Email");

        builder.HasOne(appUser => appUser.ImageFile)
            .WithMany()
            .HasForeignKey(appUser => appUser.ImageFileId);
    }
}
