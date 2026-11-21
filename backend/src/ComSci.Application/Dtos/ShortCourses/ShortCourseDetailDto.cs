using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.ShortCourses;

public class ShortCourseDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Audience { get; set; }
    public string? Location { get; set; }
    public string? TeacherName { get; set; }
    public Guid? CoverFileId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? Hours { get; set; }
    public int MaxParticipants { get; set; }
    public int RegisteredCount { get; set; }
    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }
    public ShortCourseAvailability Availability { get; set; }
}
