using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Lecturers;

public class SaveLecturerRequest
{
    [StringLength(50)]
    public string? Prefix { get; set; }
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;
    [StringLength(200)]
    public string? Position { get; set; }
    [StringLength(500)]
    public string? Expertise { get; set; }
    [StringLength(100)]
    public string? EducationLevel { get; set; }
    [StringLength(200)]
    public string? Program { get; set; }
    [StringLength(300)]
    public string? Contact { get; set; }
    public Guid? ImageFileId { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
}
