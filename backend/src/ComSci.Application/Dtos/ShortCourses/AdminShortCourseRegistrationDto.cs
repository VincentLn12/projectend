using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.ShortCourses;

public class AdminShortCourseRegistrationDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Prefix { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
