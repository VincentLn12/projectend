using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Settings;

public class QuickMenuItemDto
{
    [Required]
    [StringLength(50)]
    public string Key { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    [StringLength(100)]
    public string? Title { get; set; }
    public Guid? ImageFileId { get; set; }
}
