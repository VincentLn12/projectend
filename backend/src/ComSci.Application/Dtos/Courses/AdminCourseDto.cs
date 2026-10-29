namespace ComSci.Application.Dtos.Courses;

public class AdminCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LecturerId { get; set; }
    public string? LecturerName { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CoursePhotoDto> Photos { get; set; } = new List<CoursePhotoDto>();
    public List<GenerationDto> Generations { get; set; } = new List<GenerationDto>();
}
