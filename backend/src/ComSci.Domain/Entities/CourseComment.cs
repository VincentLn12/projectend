using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class CourseComment : AuditableEntity
{
    public string Body { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public AppUser Author { get; set; } = null!;
    public Guid GenerationId { get; set; }
    public Generation Generation { get; set; } = null!;
}
