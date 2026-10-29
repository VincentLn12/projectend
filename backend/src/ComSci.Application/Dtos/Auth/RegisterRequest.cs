using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Auth;

/// <summary>
/// ข้อมูลที่ใช้สมัครสมาชิก
/// </summary>
public class RegisterRequest
{
    /// <summary>อีเมล (ใช้เป็นชื่อผู้ใช้ด้วย)</summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>ชื่อ-นามสกุล</summary>
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>รหัสผ่าน (อย่างน้อย 8 ตัวอักษร)</summary>
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>ต้องเป็น true = ยอมรับนโยบายความเป็นส่วนตัว (PDPA)</summary>
    [Range(typeof(bool), "true", "true", ErrorMessage = "Privacy policy must be accepted.")]
    public bool AcceptPrivacy { get; set; }
}
