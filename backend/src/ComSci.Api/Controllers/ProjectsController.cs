using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Projects;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectListItemDto>>> GetList(
        [FromQuery] ProjectQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<ProjectListItemDto> result = await _projectService.GetPublishedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ProjectDetailDto result = await _projectService.GetPublishedByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}
