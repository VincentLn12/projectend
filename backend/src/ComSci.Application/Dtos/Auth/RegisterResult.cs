namespace ComSci.Application.Dtos.Auth;

/// <summary>
/// ผลการสมัครสมาชิก: สำเร็จจะมี Response ไม่สำเร็จจะมีรายการ Errors
/// </summary>
public class RegisterResult
{
    /// <summary>ผลลัพธ์เมื่อสมัครสำเร็จ (null เมื่อไม่สำเร็จ)</summary>
    public AuthResponse? Response { get; set; }

    /// <summary>ข้อความผิดพลาดจาก Identity เช่น รหัสผ่านง่ายเกินไป</summary>
    public List<string> Errors { get; set; } = new List<string>();
}
