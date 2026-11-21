using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.Property(menuItem => menuItem.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(menuItem => menuItem.Target)
            .HasMaxLength(500);

        builder.HasOne(menuItem => menuItem.Parent)
            .WithMany()
            .HasForeignKey(menuItem => menuItem.ParentId);

        builder.HasIndex(menuItem => new { menuItem.ParentId, menuItem.SortOrder });
    }
}
