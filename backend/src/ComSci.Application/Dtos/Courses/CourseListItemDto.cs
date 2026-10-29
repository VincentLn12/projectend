namespace ComSci.Application.Dtos.Courses;

public class CourseListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LecturerName { get; set; }
    public Guid? MainImageFileId { get; set; }
}
