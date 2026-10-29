using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง News (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        // ใช้ชื่อตารางในฐานข้อมูลว่า News
        builder.ToTable("News");

        // จำกัดความยาว Title ไม่เกิน 300 ตัวอักษร
        builder.Property(news => news.Title)
            .HasMaxLength(300);

        // จำกัดความยาว SubTitle ไม่เกิน 500 ตัวอักษร
        builder.Property(news => news.SubTitle)
            .HasMaxLength(500);

        // สร้าง index เพื่อให้ค้นหาเร็วขึ้น
        builder.HasIndex(news => new { news.IsPublished, news.PublishedAt });

        // ความสัมพันธ์: News.Author (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(news => news.Author)
            .WithMany()
            .HasForeignKey(news => news.AuthorId);
    }
}
