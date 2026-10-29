namespace ComSci.Application.Dtos.Auth;

/// <summary>
/// ผลลัพธ์เมื่อล็อกอิน สมัครสมาชิก หรือต่ออายุสำเร็จ
/// </summary>
public class AuthResponse
{
    /// <summary>JWT สำหรับเรียก API (ส่งใน header Authorization: Bearer ...)</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>เวลาหมดอายุของ access token (UTC)</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>refresh token สำหรับขอ access token ใหม่ (ใช้ได้ครั้งเดียว)</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>ข้อมูลผู้ใช้</summary>
    public UserInfo User { get; set; } = new UserInfo();
}
