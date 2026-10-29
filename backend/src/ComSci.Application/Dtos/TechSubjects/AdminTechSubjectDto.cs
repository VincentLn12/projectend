namespace ComSci.Application.Dtos.TechSubjects;

public class AdminTechSubjectDto
{
    public Guid Id { get; set; }
    public string? Icon { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public List<Guid> ImageFileIds { get; set; } = new List<Guid>();
    public DateTime CreatedAt { get; set; }
}
