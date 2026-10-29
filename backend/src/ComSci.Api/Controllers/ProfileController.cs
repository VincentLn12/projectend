using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComSci.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<MyProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        MyProfileDto profile = await _profileService.GetMyProfileAsync(cancellationToken);
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<MyProfileDto>> UpdateProfile(UpdateMyProfileRequest request, CancellationToken cancellationToken)
    {
        MyProfileDto profile = await _profileService.UpdateMyProfileAsync(request, cancellationToken);
        return Ok(profile);
    }

    [HttpPost("change-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _profileService.ChangeMyPasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
