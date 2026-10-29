using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/courses")]
public class AdminCoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public AdminCoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminCourseListItemDto>>> GetList(
        [FromQuery] AdminCourseQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminCourseListItemDto> result = await _courseService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminCourseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminCourseDto result = await _courseService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminCourseDto>> Create(SaveCourseRequest request, CancellationToken cancellationToken)
    {
        AdminCourseDto created = await _courseService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminCourseDto>> Update(
        Guid id,
        SaveCourseRequest request,
        CancellationToken cancellationToken)
    {
        AdminCourseDto updated = await _courseService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminCourseDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminCourseDto updated = await _courseService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _courseService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
