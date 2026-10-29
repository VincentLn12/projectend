using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Auth;

/// <summary>
/// ข้อมูลที่ใช้ต่ออายุหรือออกจากระบบ
/// </summary>
public class RefreshRequest
{
    /// <summary>refresh token ที่ได้จากตอนล็อกอิน</summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
