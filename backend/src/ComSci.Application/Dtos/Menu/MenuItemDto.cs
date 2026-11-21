using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Menu;

public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public MenuLinkKind Kind { get; set; }
    public string? Target { get; set; }
    public bool OpenInNewTab { get; set; }
    public List<MenuItemDto> Children { get; set; } = new List<MenuItemDto>();
}
