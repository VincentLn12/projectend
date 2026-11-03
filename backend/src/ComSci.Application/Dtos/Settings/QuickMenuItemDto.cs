using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Settings;

public class QuickMenuItemDto
{
    [Required]
    [StringLength(50)]
    public string Key { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
