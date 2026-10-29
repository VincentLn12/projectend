using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.Property(x => x.FullName).HasMaxLength(200);
        b.Property(x => x.Bio).HasMaxLength(1000);
        b.Property(x => x.UserType).HasConversion<byte>();
        b.HasIndex(x => x.NormalizedEmail).IsUnique().HasFilter("[NormalizedEmail] IS NOT NULL").HasDatabaseName("IX_Users_Email");
        b.HasOne(x => x.ImageFile).WithMany().HasForeignKey(x => x.ImageFileId);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.Property(x => x.TokenHash).HasMaxLength(200);
        b.HasIndex(x => x.TokenHash);
        b.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId);
    }
}

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> b)
    {
        b.HasKey(x => x.UserId);
        b.Property(x => x.StudentCode).HasMaxLength(20);
        b.Property(x => x.PreviousSchool).HasMaxLength(200);
        b.Property(x => x.Address).HasMaxLength(500);
        b.HasIndex(x => x.StudentCode).IsUnique();
        b.HasOne(x => x.User).WithOne(u => u.StudentProfile).HasForeignKey<StudentProfile>(x => x.UserId);
    }
}

public class LecturerConfiguration : IEntityTypeConfiguration<Lecturer>
{
    public void Configure(EntityTypeBuilder<Lecturer> b)
    {
        b.Property(x => x.Prefix).HasMaxLength(50);
        b.Property(x => x.FullName).HasMaxLength(200);
        b.Property(x => x.Position).HasMaxLength(200);
        b.Property(x => x.Expertise).HasMaxLength(500);
        b.Property(x => x.EducationLevel).HasMaxLength(100);
        b.Property(x => x.Program).HasMaxLength(200);
        b.Property(x => x.Contact).HasMaxLength(300);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        b.HasOne(x => x.ImageFile).WithMany().HasForeignKey(x => x.ImageFileId);
    }
}

public class FileRecordConfiguration : IEntityTypeConfiguration<FileRecord>
{
    public void Configure(EntityTypeBuilder<FileRecord> b)
    {
        b.ToTable("Files");
        b.Property(x => x.OriginalName).HasMaxLength(260);
        b.Property(x => x.StoragePath).HasMaxLength(500);
        b.Property(x => x.ContentType).HasMaxLength(100);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.Property(x => x.Action).HasMaxLength(20);
        b.Property(x => x.EntityName).HasMaxLength(100);
        b.Property(x => x.EntityId).HasMaxLength(50);
        b.HasIndex(x => new { x.EntityName, x.EntityId });
    }
}
