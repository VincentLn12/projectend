using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.ShortCourses;

public class ShortCourseRegistrationDto
{
    public Guid Id { get; set; }
    public Guid ShortCourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Prefix { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public bool CanModify { get; set; }
}
