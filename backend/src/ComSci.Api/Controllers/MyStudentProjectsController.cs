using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.StudentProjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Student)]
[Route("api/me/student-projects")]
public class MyStudentProjectsController : ControllerBase
{
    private readonly IStudentProjectService _studentProjectService;

    public MyStudentProjectsController(IStudentProjectService studentProjectService)
    {
        _studentProjectService = studentProjectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MyStudentProjectDto>>> GetMine(CancellationToken cancellationToken)
    {
        List<MyStudentProjectDto> result = await _studentProjectService.GetMineAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MyStudentProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        MyStudentProjectDto result = await _studentProjectService.GetMineByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MyStudentProjectDto>> Create(SaveStudentProjectRequest request, CancellationToken cancellationToken)
    {
        MyStudentProjectDto created = await _studentProjectService.CreateMineAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MyStudentProjectDto>> Update(Guid id, SaveStudentProjectRequest request, CancellationToken cancellationToken)
    {
        MyStudentProjectDto updated = await _studentProjectService.UpdateMineAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<MyStudentProjectDto>> SetPublished(Guid id, SetPublishedRequest request, CancellationToken cancellationToken)
    {
        MyStudentProjectDto updated = await _studentProjectService.SetMinePublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _studentProjectService.DeleteMineAsync(id, cancellationToken);
        return NoContent();
    }
}
