using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Users;

public class AdminUserListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public string? StudentCode { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
}
