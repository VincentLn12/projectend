namespace ComSci.Domain.Entities;

/// <summary>
/// refresh token ที่ใช้ต่ออายุการล็อกอิน เก็บเฉพาะค่าแฮช ไม่เก็บ token จริง
/// </summary>
public class RefreshToken
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>เจ้าของ token (FK)</summary>
    public Guid UserId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: เจ้าของ token</summary>
    public AppUser User { get; set; } = null!;

    /// <summary>ค่าแฮช SHA-256 ของ token</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>วันหมดอายุ (UTC)</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>วันที่ถูกเพิกถอน (null = ยังใช้ได้)</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>วันที่ออก token (UTC)</summary>
    public DateTime CreatedAt { get; set; }
}
