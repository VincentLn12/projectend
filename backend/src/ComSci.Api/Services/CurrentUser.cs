using System.Security.Claims;
using ComSci.Application.Abstractions;

namespace ComSci.Api.Services;

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
            // อยู่นอก request (เช่น ตอน seed ข้อมูลเริ่มระบบ)
            if (httpContext == null)
            {
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
