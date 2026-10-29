using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class Lecturer : AuditableEntity
{
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public string? Prefix { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Expertise { get; set; }
    public string? EducationLevel { get; set; }
    public string? Program { get; set; }
    public string? Contact { get; set; }
    public Guid? ImageFileId { get; set; }
    public FileRecord? ImageFile { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
}
