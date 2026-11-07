using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Student)]
[Route("api/me/projects")]
public class MyProjectsController : ControllerBase
{
    private readonly IMyProjectService _myProjectService;

    public MyProjectsController(IMyProjectService myProjectService)
    {
        _myProjectService = myProjectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminProjectListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        List<AdminProjectListItemDto> result = await _myProjectService.GetMineAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminProjectDto result = await _myProjectService.GetMineByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminProjectDto>> Create(SaveMyProjectRequest request, CancellationToken cancellationToken)
    {
        AdminProjectDto created = await _myProjectService.CreateMineAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminProjectDto>> Update(Guid id, SaveMyProjectRequest request, CancellationToken cancellationToken)
    {
        AdminProjectDto updated = await _myProjectService.UpdateMineAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminProjectDto>> SetPublished(Guid id, SetPublishedRequest request, CancellationToken cancellationToken)
    {
        AdminProjectDto updated = await _myProjectService.SetMinePublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _myProjectService.DeleteMineAsync(id, cancellationToken);
        return NoContent();
    }
}
