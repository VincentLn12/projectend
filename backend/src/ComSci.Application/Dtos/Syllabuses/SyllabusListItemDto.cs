namespace ComSci.Application.Dtos.Syllabuses;

public class SyllabusListItemDto
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
}
