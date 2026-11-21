using System.ComponentModel.DataAnnotations;
using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Menu;

public class SaveMenuItemRequest
{
    [Required]
    [StringLength(100)]
    public string Label { get; set; } = string.Empty;
    [EnumDataType(typeof(MenuLinkKind))]
    public MenuLinkKind Kind { get; set; }
    [StringLength(500)]
    public string? Target { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsVisible { get; set; }
    public bool OpenInNewTab { get; set; }
}
