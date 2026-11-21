using ComSci.Domain.Common;
using ComSci.Domain.Enums;

namespace ComSci.Domain.Entities;

public class MenuItem : AuditableEntity
{
    public string Label { get; set; } = string.Empty;
    public MenuLinkKind Kind { get; set; }
    public string? Target { get; set; }
    public Guid? ParentId { get; set; }
    public MenuItem? Parent { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public bool OpenInNewTab { get; set; }
}
