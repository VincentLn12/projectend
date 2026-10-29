using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class HomeBannerConfiguration : IEntityTypeConfiguration<HomeBanner>
{
    public void Configure(EntityTypeBuilder<HomeBanner> builder)
    {
        builder.Property(homeBanner => homeBanner.Title)
            .HasMaxLength(200);

        builder.Property(homeBanner => homeBanner.LinkUrl)
            .HasMaxLength(500);

        builder.HasOne(homeBanner => homeBanner.File)
            .WithMany()
            .HasForeignKey(homeBanner => homeBanner.FileId);
    }
}
