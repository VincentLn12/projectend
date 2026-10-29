using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Lecturers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/lecturers")]
public class AdminLecturersController : ControllerBase
{
    private readonly ILecturerService _lecturerService;

    public AdminLecturersController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminLecturerDto>>> GetList(
        [FromQuery] AdminLecturerQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminLecturerDto> result = await _lecturerService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminLecturerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminLecturerDto result = await _lecturerService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminLecturerDto>> Create(SaveLecturerRequest request, CancellationToken cancellationToken)
    {
        AdminLecturerDto created = await _lecturerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminLecturerDto>> Update(
        Guid id,
        SaveLecturerRequest request,
        CancellationToken cancellationToken)
    {
        AdminLecturerDto updated = await _lecturerService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminLecturerDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminLecturerDto updated = await _lecturerService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _lecturerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
