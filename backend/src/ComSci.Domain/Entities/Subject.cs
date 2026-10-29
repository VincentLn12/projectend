namespace ComSci.Domain.Entities;

public class Subject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SyllabusId { get; set; }
    public Syllabus Syllabus { get; set; } = null!;
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Credit { get; set; }
    public byte SubjectCategory { get; set; }
}
