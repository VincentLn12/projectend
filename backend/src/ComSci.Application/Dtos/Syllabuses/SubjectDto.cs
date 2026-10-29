namespace ComSci.Application.Dtos.Syllabuses;

public class SubjectDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Credit { get; set; }
    public byte SubjectCategory { get; set; }
}
