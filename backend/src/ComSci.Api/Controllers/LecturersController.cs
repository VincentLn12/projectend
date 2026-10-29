using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Lecturers;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/lecturers")]
public class LecturersController : ControllerBase
{
    private readonly ILecturerService _lecturerService;

    public LecturersController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LecturerDto>>> GetList(CancellationToken cancellationToken)
    {
        List<LecturerDto> lecturers = await _lecturerService.GetPublishedAsync(cancellationToken);
        return Ok(lecturers);
    }
}
