using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Auth;

/// <summary>
/// ข้อมูลที่ใช้ล็อกอิน
/// </summary>
public class LoginRequest
{
    /// <summary>อีเมล ชื่อผู้ใช้ หรือรหัสนักศึกษา</summary>
    [Required]
    [StringLength(256)]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>รหัสผ่าน</summary>
    [Required]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
