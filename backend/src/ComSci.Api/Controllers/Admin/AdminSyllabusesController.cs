using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Syllabuses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/syllabuses")]
public class AdminSyllabusesController : ControllerBase
{
    private readonly ISyllabusService _syllabusService;

    public AdminSyllabusesController(ISyllabusService syllabusService)
    {
        _syllabusService = syllabusService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminSyllabusListItemDto>>> GetList(
        [FromQuery] AdminSyllabusQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminSyllabusListItemDto> result = await _syllabusService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminSyllabusDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminSyllabusDto result = await _syllabusService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminSyllabusDto>> Create(SaveSyllabusRequest request, CancellationToken cancellationToken)
    {
        AdminSyllabusDto created = await _syllabusService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminSyllabusDto>> Update(
        Guid id,
        SaveSyllabusRequest request,
        CancellationToken cancellationToken)
    {
        AdminSyllabusDto updated = await _syllabusService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminSyllabusDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminSyllabusDto updated = await _syllabusService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _syllabusService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
