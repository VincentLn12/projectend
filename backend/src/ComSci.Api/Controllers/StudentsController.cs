using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Profiles;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IProfileService _profileService;

    public StudentsController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StudentListItemDto>>> GetList(
        [FromQuery] StudentQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<StudentListItemDto> result = await _profileService.GetStudentsAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        StudentDetailDto result = await _profileService.GetStudentAsync(id, cancellationToken);
        return Ok(result);
    }
}
