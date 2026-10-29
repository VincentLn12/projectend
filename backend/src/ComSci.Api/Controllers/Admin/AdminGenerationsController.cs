using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin")]
public class AdminGenerationsController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IRegistrationService _registrationService;

    public AdminGenerationsController(ICourseService courseService, IRegistrationService registrationService)
    {
        _courseService = courseService;
        _registrationService = registrationService;
    }

    [HttpPost("courses/{courseId:guid}/generations")]
    public async Task<ActionResult<GenerationDto>> Create(
        Guid courseId,
        SaveGenerationRequest request,
        CancellationToken cancellationToken)
    {
        GenerationDto created = await _courseService.CreateGenerationAsync(courseId, request, cancellationToken);
        return Ok(created);
    }

    [HttpPut("generations/{id:guid}")]
    public async Task<ActionResult<GenerationDto>> Update(
        Guid id,
        SaveGenerationRequest request,
        CancellationToken cancellationToken)
    {
        GenerationDto updated = await _courseService.UpdateGenerationAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("generations/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _courseService.DeleteGenerationAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("generations/{id:guid}/registrations")]
    public async Task<ActionResult<List<RegistrationListItemDto>>> GetRegistrations(Guid id, CancellationToken cancellationToken)
    {
        List<RegistrationListItemDto> result = await _registrationService.GetGenerationRegistrationsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("generations/{id:guid}/registrations/{userId:guid}")]
    public async Task<IActionResult> CancelRegistration(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _registrationService.CancelRegistrationAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
