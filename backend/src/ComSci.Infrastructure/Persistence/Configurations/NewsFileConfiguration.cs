using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class NewsFileConfiguration : IEntityTypeConfiguration<NewsFile>
{
    public void Configure(EntityTypeBuilder<NewsFile> builder)
    {
        builder.HasOne(newsFile => newsFile.News)
            .WithMany(news => news.Files)
            .HasForeignKey(newsFile => newsFile.NewsId);

        builder.HasOne(newsFile => newsFile.File)
            .WithMany()
            .HasForeignKey(newsFile => newsFile.FileId);
    }
}
