namespace ComSci.Application.Dtos.Courses;

public class CourseDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LecturerName { get; set; }
    public List<Guid> ImageFileIds { get; set; } = new List<Guid>();
    public List<GenerationDto> Generations { get; set; } = new List<GenerationDto>();
}
