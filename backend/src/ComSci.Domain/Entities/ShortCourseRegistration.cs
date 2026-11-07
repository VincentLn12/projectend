using ComSci.Domain.Common;
using ComSci.Domain.Enums;

namespace ComSci.Domain.Entities;

public class ShortCourseRegistration : AuditableEntity
{
    public Guid ShortCourseId { get; set; }
    public ShortCourse ShortCourse { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string? Prefix { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;
    public DateTime RegisteredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
