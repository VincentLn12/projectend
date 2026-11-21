using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/students")]
public class AdminStudentsController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public AdminStudentsController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminStudentListItemDto>>> GetList(
        [FromQuery] AdminStudentQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminStudentListItemDto> result = await _userAdminService.GetStudentsAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminStudentListItemDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminStudentListItemDto student = await _userAdminService.GetStudentByIdAsync(id, cancellationToken);
        return Ok(student);
    }

    [HttpGet("entry-years")]
    public async Task<ActionResult<List<short>>> GetEntryYears(CancellationToken cancellationToken)
    {
        List<short> years = await _userAdminService.GetStudentEntryYearsAsync(cancellationToken);
        return Ok(years);
    }
}
