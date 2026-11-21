using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.TechSubjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/tech-subjects")]
public class AdminTechSubjectsController : ControllerBase
{
    private readonly ITechSubjectService _subjectService;

    public AdminTechSubjectsController(ITechSubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminTechSubjectDto>>> GetList(CancellationToken cancellationToken)
    {
        List<AdminTechSubjectDto> subjects = await _subjectService.GetAllAsync(cancellationToken);
        return Ok(subjects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminTechSubjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminTechSubjectDto subject = await _subjectService.GetByIdAsync(id, cancellationToken);
        return Ok(subject);
    }

    [HttpPost]
    public async Task<ActionResult<AdminTechSubjectDto>> Create(SaveTechSubjectRequest request, CancellationToken cancellationToken)
    {
        AdminTechSubjectDto created = await _subjectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminTechSubjectDto>> Update(
        Guid id,
        SaveTechSubjectRequest request,
        CancellationToken cancellationToken)
    {
        AdminTechSubjectDto updated = await _subjectService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminTechSubjectDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminTechSubjectDto updated = await _subjectService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:guid}/move")]
    public async Task<IActionResult> Move(Guid id, MoveTechSubjectRequest request, CancellationToken cancellationToken)
    {
        await _subjectService.MoveAsync(id, request.Direction == "up", cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _subjectService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
