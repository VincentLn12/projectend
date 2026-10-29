using ComSci.Domain.Enums;

namespace ComSci.Domain.Entities;

public class CourseRegistration
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid GenerationId { get; set; }
    public Generation Generation { get; set; } = null!;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
    public DateTime RegisteredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
