using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.ShortCourses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/short-courses")]
public class ShortCoursesController : ControllerBase
{
    private readonly IShortCourseService _shortCourseService;
    private readonly IShortCourseRegistrationService _registrationService;

    public ShortCoursesController(IShortCourseService shortCourseService, IShortCourseRegistrationService registrationService)
    {
        _shortCourseService = shortCourseService;
        _registrationService = registrationService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ShortCourseListItemDto>>> GetList(
        [FromQuery] PageQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<ShortCourseListItemDto> result = await _shortCourseService.GetPublishedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShortCourseDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ShortCourseDetailDto result = await _shortCourseService.GetPublishedByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/registrations")]
    [EnableRateLimiting("public-registration")]
    public async Task<ActionResult<ShortCourseRegistrationDto>> Register(
        Guid id,
        RegisterShortCourseRequest request,
        CancellationToken cancellationToken)
    {
        ShortCourseRegistrationDto result = await _registrationService.RegisterAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
