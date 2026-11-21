using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/menu")]
public class AdminMenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public AdminMenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminMenuItemDto>>> GetList(CancellationToken cancellationToken)
    {
        List<AdminMenuItemDto> items = await _menuService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("pages")]
    public ActionResult<IEnumerable<MenuPageOption>> GetPages()
    {
        return Ok(MenuPages.All);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminMenuItemDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminMenuItemDto item = await _menuService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<AdminMenuItemDto>> Create(SaveMenuItemRequest request, CancellationToken cancellationToken)
    {
        AdminMenuItemDto created = await _menuService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminMenuItemDto>> Update(
        Guid id,
        SaveMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        AdminMenuItemDto updated = await _menuService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/visible")]
    public async Task<ActionResult<AdminMenuItemDto>> SetVisible(
        Guid id,
        SetVisibleRequest request,
        CancellationToken cancellationToken)
    {
        AdminMenuItemDto updated = await _menuService.SetVisibleAsync(id, request.IsVisible, cancellationToken);
        return Ok(updated);
    }

    [HttpPut("order")]
    public async Task<IActionResult> Reorder(ReorderMenuRequest request, CancellationToken cancellationToken)
    {
        await _menuService.ReorderAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _menuService.ResetToDefaultAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _menuService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
