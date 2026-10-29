using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Courses;

public class RegistrationListItemDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
