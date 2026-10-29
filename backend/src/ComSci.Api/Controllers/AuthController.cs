using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api")]
public class AuthController(IAuthService auth, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("auth/login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        return result is null ? Problem("Invalid credentials.", statusCode: StatusCodes.Status401Unauthorized) : result;
    }

    [HttpPost("auth/register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var (response, errors) = await auth.RegisterAsync(request, ct);
        if (response is not null) return response;

        foreach (var error in errors) ModelState.AddModelError(nameof(request.Password), error);
        return ValidationProblem(ModelState);
    }

    [HttpPost("auth/refresh")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var result = await auth.RefreshAsync(request.RefreshToken, ct);
        return result is null ? Problem("Invalid refresh token.", statusCode: StatusCodes.Status401Unauthorized) : result;
    }

    [HttpPost("auth/logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> Me(CancellationToken ct)
    {
        if (currentUser.UserId is not { } id) return Unauthorized();
        var info = await auth.GetUserAsync(id, ct);
        return info is null ? Unauthorized() : info;
    }
}
