using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.StudentProjects;

public class SaveStudentProjectRequest
{
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(1900, 2700)]
    public short AcademicYear { get; set; }
    [Range(1, 3)]
    public byte Semester { get; set; }
    [StringLength(200)]
    public string? CourseName { get; set; }
    [StringLength(500)]
    public string? Technologies { get; set; }
    [StringLength(500)]
    public string? TeamMembers { get; set; }
    public Guid? LecturerId { get; set; }
    public Guid? ImageFileId { get; set; }
    public Guid? PdfFileId { get; set; }
    [Url]
    [StringLength(500)]
    public string? VideoUrl { get; set; }
    [Url]
    [StringLength(500)]
    public string? WebUrl { get; set; }
    [Url]
    [StringLength(500)]
    public string? GithubUrl { get; set; }
    public bool IsPublished { get; set; }
}
