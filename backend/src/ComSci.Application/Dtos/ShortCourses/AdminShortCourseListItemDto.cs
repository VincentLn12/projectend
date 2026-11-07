using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.ShortCourses;

public class AdminShortCourseListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxParticipants { get; set; }
    public int RegisteredCount { get; set; }
    public bool IsPublished { get; set; }
    public ShortCourseAvailability Availability { get; set; }
}
