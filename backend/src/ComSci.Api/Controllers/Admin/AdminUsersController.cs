using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public AdminUsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserListItemDto>>> GetList(
        [FromQuery] AdminUserQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminUserListItemDto> result = await _userAdminService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminUserDto result = await _userAdminService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        AdminUserDto created = await _userAdminService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminUserDto>> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        AdminUserDto updated = await _userAdminService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _userAdminService.ResetPasswordAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/locked")]
    public async Task<IActionResult> SetLocked(
        Guid id,
        SetLockedRequest request,
        CancellationToken cancellationToken)
    {
        await _userAdminService.SetLockedAsync(id, request.IsLocked, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _userAdminService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
