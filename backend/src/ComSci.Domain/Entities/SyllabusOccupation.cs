namespace ComSci.Domain.Entities;

public class SyllabusOccupation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SyllabusId { get; set; }
    public Syllabus Syllabus { get; set; } = null!;
    public string Sentence { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
