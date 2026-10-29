using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Profiles;

public class UpdateMyProfileRequest
{
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? Bio { get; set; }
    public Guid? ImageFileId { get; set; }
    public bool ShowEmail { get; set; }
    public bool ShowProfile { get; set; }
    [StringLength(200)]
    public string? PreviousSchool { get; set; }
    [StringLength(500)]
    public string? Address { get; set; }
}
