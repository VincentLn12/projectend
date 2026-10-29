using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.TechSubjects;

public class SaveTechSubjectRequest
{
    [StringLength(100)]
    public string? Icon { get; set; }
    [Required]
    [StringLength(200)]
    public string SubjectName { get; set; } = string.Empty;
    [StringLength(300)]
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public List<Guid>? FileIds { get; set; }
}
