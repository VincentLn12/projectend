using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Courses;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CourseListItemDto>>> GetList(
        [FromQuery] PageQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<CourseListItemDto> result = await _courseService.GetPublishedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        CourseDetailDto result = await _courseService.GetPublishedByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}
