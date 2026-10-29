using System.Security.Claims;
using ComSci.Application.Abstractions;

namespace ComSci.Api.Services;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
