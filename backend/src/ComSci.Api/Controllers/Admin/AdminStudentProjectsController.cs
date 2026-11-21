using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.StudentProjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/student-projects")]
public class AdminStudentProjectsController : ControllerBase
{
    private readonly IStudentProjectService _studentProjectService;

    public AdminStudentProjectsController(IStudentProjectService studentProjectService)
    {
        _studentProjectService = studentProjectService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminStudentProjectListItemDto>>> GetList(
        [FromQuery] AdminStudentProjectQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminStudentProjectListItemDto> result = await _studentProjectService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<IActionResult> SetPublished(Guid id, SetPublishedRequest request, CancellationToken cancellationToken)
    {
        await _studentProjectService.SetPublishedByAdminAsync(id, request.IsPublished, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _studentProjectService.DeleteByAdminAsync(id, cancellationToken);
        return NoContent();
    }
}
