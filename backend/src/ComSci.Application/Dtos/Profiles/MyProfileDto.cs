using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Profiles;

public class MyProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public Guid? ImageFileId { get; set; }
    public UserType UserType { get; set; }
    public bool ShowEmail { get; set; }
    public bool ShowProfile { get; set; }
    public DateTime? ConsentAt { get; set; }
    public StudentProfileDto? Student { get; set; }
}
