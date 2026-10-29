using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> b)
    {
        b.ToTable("News");
        b.Property(x => x.Title).HasMaxLength(300);
        b.Property(x => x.SubTitle).HasMaxLength(500);
        b.HasIndex(x => new { x.IsPublished, x.PublishedAt });
        b.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId);
    }
}

public class NewsFileConfiguration : IEntityTypeConfiguration<NewsFile>
{
    public void Configure(EntityTypeBuilder<NewsFile> b)
    {
        b.HasOne(x => x.News).WithMany(n => n.Files).HasForeignKey(x => x.NewsId);
        b.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId);
    }
}

public class HomeBannerConfiguration : IEntityTypeConfiguration<HomeBanner>
{
    public void Configure(EntityTypeBuilder<HomeBanner> b)
    {
        b.Property(x => x.Title).HasMaxLength(200);
        b.Property(x => x.LinkUrl).HasMaxLength(500);
        b.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId);
    }
}

public class TechSubjectConfiguration : IEntityTypeConfiguration<TechSubject>
{
    public void Configure(EntityTypeBuilder<TechSubject> b)
    {
        b.Property(x => x.Icon).HasMaxLength(100);
        b.Property(x => x.SubjectName).HasMaxLength(200);
        b.Property(x => x.SubTitle).HasMaxLength(300);
    }
}

public class TechSubjectFileConfiguration : IEntityTypeConfiguration<TechSubjectFile>
{
    public void Configure(EntityTypeBuilder<TechSubjectFile> b)
    {
        b.HasOne(x => x.TechSubject).WithMany(t => t.Files).HasForeignKey(x => x.TechSubjectId);
        b.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId);
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> b)
    {
        b.Property(x => x.WebName).HasMaxLength(200);
        b.Property(x => x.RegisterUrl).HasMaxLength(500);
        b.Property(x => x.KruUrl).HasMaxLength(500);
        b.Property(x => x.PageFacebook).HasMaxLength(500);
        b.Property(x => x.VideoUrl).HasMaxLength(500);
        b.Property(x => x.MapLocation).HasMaxLength(500);
        b.HasOne(x => x.LogoFile).WithMany().HasForeignKey(x => x.LogoFileId);
    }
}
