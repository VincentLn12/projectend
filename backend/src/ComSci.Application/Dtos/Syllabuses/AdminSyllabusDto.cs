namespace ComSci.Application.Dtos.Syllabuses;

public class AdminSyllabusDto
{
    public Guid Id { get; set; }
    public string NameTH { get; set; } = string.Empty;
    public string? NameEN { get; set; }
    public short Year { get; set; }
    public string? DegreeTH { get; set; }
    public string? DegreeEN { get; set; }
    public short? TotalCredits { get; set; }
    public Guid? PdfFileId { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Objectives { get; set; } = new List<string>();
    public List<string> Occupations { get; set; } = new List<string>();
    public List<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();
}
