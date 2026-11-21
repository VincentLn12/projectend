using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/projects")]
public class AdminProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public AdminProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminProjectListItemDto>>> GetList(
        [FromQuery] AdminProjectQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminProjectListItemDto> result = await _projectService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("academic-years")]
    public async Task<ActionResult<IReadOnlyList<short>>> GetAcademicYears(CancellationToken cancellationToken)
    {
        IReadOnlyList<short> years = await _projectService.GetAcademicYearsAsync(cancellationToken);
        return Ok(years);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminProjectDto result = await _projectService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminProjectDto>> Create(SaveProjectRequest request, CancellationToken cancellationToken)
    {
        AdminProjectDto created = await _projectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminProjectDto>> Update(
        Guid id,
        SaveProjectRequest request,
        CancellationToken cancellationToken)
    {
        AdminProjectDto updated = await _projectService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminProjectDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminProjectDto updated = await _projectService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _projectService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
