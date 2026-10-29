using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Courses;

public class MyRegistrationDto
{
    public Guid GenerationId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? GenerationName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public RegistrationStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
}
