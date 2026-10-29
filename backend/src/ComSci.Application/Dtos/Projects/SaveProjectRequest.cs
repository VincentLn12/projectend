using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Projects;

public class SaveProjectRequest
{
    [Required]
    [StringLength(300)]
    public string NameTH { get; set; } = string.Empty;
    [StringLength(300)]
    public string? NameEN { get; set; }
    public string? Description { get; set; }
    [Range(1900, 2700)]
    public short AcademicYear { get; set; }
    [StringLength(500)]
    public string? KeyWords { get; set; }
    [Url]
    [StringLength(500)]
    public string? VideoUrl { get; set; }
    [Url]
    [StringLength(500)]
    public string? WebUrl { get; set; }
    [Url]
    [StringLength(500)]
    public string? GithubUrl { get; set; }
    public Guid? ImageFileId { get; set; }
    public Guid? PdfFileId { get; set; }
    [Required]
    public Guid StudentId { get; set; }
    public bool IsPublished { get; set; }
    public List<ProjectAdvisorInput>? Advisors { get; set; }
}
