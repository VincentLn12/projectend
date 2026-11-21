namespace ComSci.Domain.Entities;

public class SystemSetting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string WebName { get; set; } = string.Empty;
    public string? RegisterUrl { get; set; }
    public string? KruUrl { get; set; }
    public string? PageFacebook { get; set; }
    public Guid? LogoFileId { get; set; }
    public FileRecord? LogoFile { get; set; }
    public string? VideoUrl { get; set; }
    public string? MapLocation { get; set; }
    public string? UniversityName { get; set; }
    public string? FooterText { get; set; }
}
