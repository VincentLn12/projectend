using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class Syllabus : AuditableEntity
{
    public string NameTH { get; set; } = string.Empty;
    public string? NameEN { get; set; }
    public short Year { get; set; }
    public string? DegreeTH { get; set; }
    public string? DegreeEN { get; set; }
    public short? TotalCredits { get; set; }
    public Guid? PdfFileId { get; set; }
    public FileRecord? PdfFile { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<SyllabusObjective> Objectives { get; set; } = new List<SyllabusObjective>();
    public ICollection<SyllabusOccupation> Occupations { get; set; } = new List<SyllabusOccupation>();
}
