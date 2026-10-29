using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class TechSubject : AuditableEntity
{
    public string? Icon { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<TechSubjectFile> Files { get; set; } = new List<TechSubjectFile>();
}
