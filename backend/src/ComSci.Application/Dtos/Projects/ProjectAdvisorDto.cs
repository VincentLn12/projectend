using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Projects;

public class ProjectAdvisorDto
{
    public Guid LecturerId { get; set; }
    public string LecturerName { get; set; } = string.Empty;
    public string? LecturerPrefix { get; set; }
    public AdvisorRole Role { get; set; }
}
