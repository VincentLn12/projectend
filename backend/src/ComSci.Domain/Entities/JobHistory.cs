using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class JobHistory : AuditableEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string JobName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Position { get; set; }
    public DateOnly StartDate { get; set; }
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
}
