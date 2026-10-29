using ComSci.Application.Dtos.Profiles;
using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Users;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public Guid? ImageFileId { get; set; }
    public UserType UserType { get; set; }
    public bool IsLocked { get; set; }
    public bool ShowEmail { get; set; }
    public bool ShowProfile { get; set; }
    public DateTime? ConsentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public StudentProfileDto? Student { get; set; }
}
