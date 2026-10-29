using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("auth/login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        AuthResponse? response = await _authService.LoginAsync(request, cancellationToken);
        if (response == null)
        {
            // ตอบข้อความเดียวกันทุกกรณี ไม่บอกว่าผิดที่ชื่อผู้ใช้หรือรหัสผ่าน
            return Problem(detail: "Invalid credentials.", statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(response);
    }

    [HttpPost("auth/register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        RegisterResult result = await _authService.RegisterAsync(request, cancellationToken);
        if (result.Response != null)
        {
            return Ok(result.Response);
        }

        foreach (string error in result.Errors)
        {
            ModelState.AddModelError("Password", error);
        }
        return ValidationProblem(ModelState);
    }

    [HttpPost("auth/refresh")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        AuthResponse? response = await _authService.RefreshAsync(request.RefreshToken, cancellationToken);
        if (response == null)
        {
            return Problem(detail: "Invalid refresh token.", statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(response);
    }

    [HttpPost("auth/logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> GetMe(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        UserInfo? userInfo = await _authService.GetUserInfoAsync(_currentUser.UserId.Value, cancellationToken);
        if (userInfo == null)
        {
            return Unauthorized();
        }

        return Ok(userInfo);
    }
}
