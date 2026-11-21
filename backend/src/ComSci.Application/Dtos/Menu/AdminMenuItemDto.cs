using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Menu;

public class AdminMenuItemDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public MenuLinkKind Kind { get; set; }
    public string? Target { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public bool OpenInNewTab { get; set; }
}
