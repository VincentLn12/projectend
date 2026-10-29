using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class Project : AuditableEntity
{
    public string NameTH { get; set; } = string.Empty;
    public string? NameEN { get; set; }
    public string? Description { get; set; }
    public short AcademicYear { get; set; }
    public string? KeyWords { get; set; }
    public string? VideoUrl { get; set; }
    public string? WebUrl { get; set; }
    public string? GithubUrl { get; set; }
    public Guid? ImageFileId { get; set; }
    public FileRecord? ImageFile { get; set; }
    public Guid? PdfFileId { get; set; }
    public FileRecord? PdfFile { get; set; }
    public Guid StudentId { get; set; }
    public AppUser Student { get; set; } = null!;
    public bool IsPublished { get; set; }
    public ICollection<ProjectAdvisor> Advisors { get; set; } = new List<ProjectAdvisor>();
}
