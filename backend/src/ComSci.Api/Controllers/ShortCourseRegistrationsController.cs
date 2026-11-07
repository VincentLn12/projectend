using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.ShortCourses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComSci.Api.Controllers;

[ApiController]
[EnableRateLimiting("public-registration")]
[Route("api/short-course-registrations")]
public class ShortCourseRegistrationsController : ControllerBase
{
    private readonly IShortCourseRegistrationService _registrationService;

    public ShortCourseRegistrationsController(IShortCourseRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost("lookup")]
    public async Task<ActionResult<ShortCourseRegistrationDto>> Lookup(
        LookupShortCourseRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        ShortCourseRegistrationDto result = await _registrationService.LookupAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{code}")]
    public async Task<ActionResult<ShortCourseRegistrationDto>> Update(
        string code,
        UpdateShortCourseRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        ShortCourseRegistrationDto result = await _registrationService.UpdateAsync(code, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{code}/cancel")]
    public async Task<IActionResult> Cancel(
        string code,
        CancelShortCourseRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        await _registrationService.CancelAsync(code, request, cancellationToken);
        return NoContent();
    }
}
