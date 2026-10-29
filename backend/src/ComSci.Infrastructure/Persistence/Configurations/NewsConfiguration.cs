using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.ToTable("News");

        builder.Property(news => news.Title)
            .HasMaxLength(300);

        builder.Property(news => news.SubTitle)
            .HasMaxLength(500);

        builder.HasIndex(news => new { news.IsPublished, news.PublishedAt });

        builder.HasOne(news => news.Author)
            .WithMany()
            .HasForeignKey(news => news.AuthorId);
    }
}
