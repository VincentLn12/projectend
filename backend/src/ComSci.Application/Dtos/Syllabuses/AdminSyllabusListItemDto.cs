namespace ComSci.Application.Dtos.Syllabuses;

public class AdminSyllabusListItemDto
{
    public Guid Id { get; set; }
    public string NameTH { get; set; } = string.Empty;
    public short Year { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
