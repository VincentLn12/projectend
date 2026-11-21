using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Menu;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/menu")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MenuItemDto>>> GetMenu(CancellationToken cancellationToken)
    {
        List<MenuItemDto> menu = await _menuService.GetPublicAsync(cancellationToken);
        return Ok(menu);
    }
}
