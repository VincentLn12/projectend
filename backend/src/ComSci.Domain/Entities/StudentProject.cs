using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class StudentProject : AuditableEntity
{
    public Guid StudentId { get; set; }
    public AppUser Student { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short AcademicYear { get; set; }
    public byte Semester { get; set; }
    public string? CourseName { get; set; }
    public string? Technologies { get; set; }
    public Guid? LecturerId { get; set; }
    public Lecturer? Lecturer { get; set; }
    public Guid? ImageFileId { get; set; }
    public FileRecord? ImageFile { get; set; }
    public Guid? PdfFileId { get; set; }
    public FileRecord? PdfFile { get; set; }
    public string? VideoUrl { get; set; }
    public string? WebUrl { get; set; }
    public string? GithubUrl { get; set; }
    public bool IsPublished { get; set; }
}
