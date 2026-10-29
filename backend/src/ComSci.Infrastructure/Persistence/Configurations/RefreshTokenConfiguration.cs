using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

/// <summary>
/// ตั้งค่าตาราง RefreshToken (ความยาวคอลัมน์ index และความสัมพันธ์)
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // จำกัดความยาว TokenHash ไม่เกิน 200 ตัวอักษร
        builder.Property(refreshToken => refreshToken.TokenHash)
            .HasMaxLength(200);

        // สร้าง index เพื่อให้ค้นหาเร็วขึ้น
        builder.HasIndex(refreshToken => refreshToken.TokenHash);

        // ความสัมพันธ์: RefreshToken.User (ไม่ cascade delete ตามกฎใน AppDbContext)
        builder.HasOne(refreshToken => refreshToken.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(refreshToken => refreshToken.UserId);
    }
}
