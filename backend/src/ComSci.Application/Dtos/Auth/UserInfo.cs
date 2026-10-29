namespace ComSci.Application.Dtos.Auth;

/// <summary>
/// ข้อมูลผู้ใช้ที่ส่งกลับให้ client
/// </summary>
public class UserInfo
{
    /// <summary>รหัสผู้ใช้</summary>
    public Guid Id { get; set; }

    /// <summary>ชื่อ-นามสกุล</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>อีเมล</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>บทบาทของผู้ใช้</summary>
    public List<string> Roles { get; set; } = new List<string>();
}
