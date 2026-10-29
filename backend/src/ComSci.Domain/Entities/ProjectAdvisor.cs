using ComSci.Domain.Enums;

namespace ComSci.Domain.Entities;

public class ProjectAdvisor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid LecturerId { get; set; }
    public Lecturer Lecturer { get; set; } = null!;
    public AdvisorRole Role { get; set; } = AdvisorRole.Main;
}
