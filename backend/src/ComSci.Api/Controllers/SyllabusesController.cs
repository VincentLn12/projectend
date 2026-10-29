using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Syllabuses;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/syllabuses")]
public class SyllabusesController : ControllerBase
{
    private readonly ISyllabusService _syllabusService;

    public SyllabusesController(ISyllabusService syllabusService)
    {
        _syllabusService = syllabusService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SyllabusListItemDto>>> GetList(CancellationToken cancellationToken)
    {
        List<SyllabusListItemDto> syllabuses = await _syllabusService.GetPublishedAsync(cancellationToken);
        return Ok(syllabuses);
    }

    [HttpGet("current")]
    public async Task<ActionResult<SyllabusDetailDto>> GetCurrent(CancellationToken cancellationToken)
    {
        SyllabusDetailDto syllabus = await _syllabusService.GetCurrentAsync(cancellationToken);
        return Ok(syllabus);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SyllabusDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        SyllabusDetailDto syllabus = await _syllabusService.GetPublishedByIdAsync(id, cancellationToken);
        return Ok(syllabus);
    }
}
