using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.StudentProjects;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/student-projects")]
public class StudentProjectsController : ControllerBase
{
    private readonly IStudentProjectService _studentProjectService;

    public StudentProjectsController(IStudentProjectService studentProjectService)
    {
        _studentProjectService = studentProjectService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StudentProjectListItemDto>>> GetList(
        [FromQuery] StudentProjectQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<StudentProjectListItemDto> result = await _studentProjectService.GetPublishedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentProjectDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        StudentProjectDetailDto result = await _studentProjectService.GetPublishedByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}
