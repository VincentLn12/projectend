using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง NewsFile (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class NewsFileConfiguration : IEntityTypeConfiguration<NewsFile>
{
    public void Configure(EntityTypeBuilder<NewsFile> builder)
    {
        // ความสัมพันธ์: NewsFile.News (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(newsFile => newsFile.News)
            .WithMany(news => news.Files)
            .HasForeignKey(newsFile => newsFile.NewsId);

        // ความสัมพันธ์: NewsFile.File (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(newsFile => newsFile.File)
            .WithMany()
            .HasForeignKey(newsFile => newsFile.FileId);
    }
}
