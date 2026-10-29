using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class Course : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LecturerId { get; set; }
    public Lecturer? Lecturer { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<CoursePhoto> Photos { get; set; } = new List<CoursePhoto>();
    public ICollection<Generation> Generations { get; set; } = new List<Generation>();
}
