using System.Security.Claims;
using ComSci.Application.Abstractions;

namespace ComSci.Api.Services;

/// <summary>
/// อ่านรหัสผู้ใช้ที่ล็อกอินอยู่จาก JWT ของ request ปัจจุบัน
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                // ไม่ได้อยู่ใน request (เช่น ตอน seed ข้อมูลเริ่มระบบ)
                return null;
            }

            string? userIdText = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdText, out Guid userId))
            {
                return userId;
            }

            return null;
        }
    }
}
