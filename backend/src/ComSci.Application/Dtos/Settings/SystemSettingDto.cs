namespace ComSci.Application.Dtos.Settings;

public class SystemSettingDto
{
    public string WebName { get; set; } = string.Empty;
    public string? RegisterUrl { get; set; }
    public string? KruUrl { get; set; }
    public string? PageFacebook { get; set; }
    public Guid? LogoFileId { get; set; }
    public string? VideoUrl { get; set; }
    public string? MapLocation { get; set; }
}
