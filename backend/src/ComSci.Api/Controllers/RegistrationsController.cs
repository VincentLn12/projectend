using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost("generations/{generationId:guid}/register")]
    public async Task<ActionResult<MyRegistrationDto>> Register(Guid generationId, CancellationToken cancellationToken)
    {
        MyRegistrationDto result = await _registrationService.RegisterAsync(generationId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("generations/{generationId:guid}/register")]
    public async Task<IActionResult> CancelMyRegistration(Guid generationId, CancellationToken cancellationToken)
    {
        await _registrationService.CancelMyRegistrationAsync(generationId, cancellationToken);
        return NoContent();
    }

    [HttpGet("me/registrations")]
    public async Task<ActionResult<List<MyRegistrationDto>>> GetMyRegistrations(CancellationToken cancellationToken)
    {
        List<MyRegistrationDto> result = await _registrationService.GetMyRegistrationsAsync(cancellationToken);
        return Ok(result);
    }
}
