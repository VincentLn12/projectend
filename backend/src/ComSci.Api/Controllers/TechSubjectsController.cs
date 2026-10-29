using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.TechSubjects;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/tech-subjects")]
public class TechSubjectsController : ControllerBase
{
    private readonly ITechSubjectService _subjectService;

    public TechSubjectsController(ITechSubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TechSubjectListItemDto>>> GetList(CancellationToken cancellationToken)
    {
        List<TechSubjectListItemDto> subjects = await _subjectService.GetPublishedAsync(cancellationToken);
        return Ok(subjects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TechSubjectDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        TechSubjectDetailDto subject = await _subjectService.GetPublishedByIdAsync(id, cancellationToken);
        return Ok(subject);
    }
}
