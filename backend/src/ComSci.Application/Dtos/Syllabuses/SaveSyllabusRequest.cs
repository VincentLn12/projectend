using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Syllabuses;

public class SaveSyllabusRequest
{
    [Required]
    [StringLength(300)]
    public string NameTH { get; set; } = string.Empty;
    [StringLength(300)]
    public string? NameEN { get; set; }
    [Range(1900, 2700)]
    public short Year { get; set; }
    [StringLength(200)]
    public string? DegreeTH { get; set; }
    [StringLength(200)]
    public string? DegreeEN { get; set; }
    [Range(0, 400)]
    public short? TotalCredits { get; set; }
    public Guid? PdfFileId { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsPublished { get; set; }
    public List<string>? Objectives { get; set; }
    public List<string>? Occupations { get; set; }
    public List<SubjectInput>? Subjects { get; set; }
}
