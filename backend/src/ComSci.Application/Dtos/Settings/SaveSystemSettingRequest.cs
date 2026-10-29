using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Settings;

public class SaveSystemSettingRequest
{
    [Required]
    [StringLength(200)]
    public string WebName { get; set; } = string.Empty;
    [Url]
    [StringLength(500)]
    public string? RegisterUrl { get; set; }
    [Url]
    [StringLength(500)]
    public string? KruUrl { get; set; }
    [Url]
    [StringLength(500)]
    public string? PageFacebook { get; set; }
    public Guid? LogoFileId { get; set; }
    [Url]
    [StringLength(500)]
    public string? VideoUrl { get; set; }
    [StringLength(500)]
    public string? MapLocation { get; set; }
}
