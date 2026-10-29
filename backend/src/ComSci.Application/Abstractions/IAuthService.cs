using ComSci.Application.Dtos.Auth;

namespace ComSci.Application.Abstractions;

/// <summary>
/// ระบบล็อกอิน สมัครสมาชิก และออก/ต่ออายุ token
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// ล็อกอินด้วยอีเมล ชื่อผู้ใช้ หรือรหัสนักศึกษา
    /// </summary>
    /// <returns>ผลลัพธ์พร้อม token หรือ null ถ้ารหัสผ่านผิดหรือบัญชีถูกล็อก</returns>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// สมัครสมาชิกใหม่ (ได้ role User) และล็อกอินให้ทันทีเมื่อสำเร็จ
    /// </summary>
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// ใช้ refresh token ขอ access token ใหม่ โดย refresh token เดิมจะใช้ไม่ได้อีก
    /// </summary>
    /// <returns>ผลลัพธ์ใหม่ หรือ null ถ้า token ไม่รู้จัก หมดอายุ หรือถูกใช้ไปแล้ว</returns>
    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// ออกจากระบบ โดยเพิกถอน refresh token นี้
    /// </summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// ดึงข้อมูลผู้ใช้จากรหัส
    /// </summary>
    /// <returns>ข้อมูลผู้ใช้ หรือ null ถ้าไม่พบ</returns>
    Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken);
}
