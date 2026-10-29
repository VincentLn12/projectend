namespace ComSci.Application.Dtos.Courses;

public class AdminCourseListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? LecturerName { get; set; }
    public bool IsPublished { get; set; }
    public int GenerationCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
