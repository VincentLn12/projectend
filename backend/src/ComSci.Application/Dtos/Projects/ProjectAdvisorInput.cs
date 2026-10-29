using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Projects;

public class ProjectAdvisorInput
{
    public Guid LecturerId { get; set; }
    public AdvisorRole Role { get; set; } = AdvisorRole.Main;
}
