namespace ComSci.Application.Dtos.TechSubjects;

public class TechSubjectDetailDto
{
    public Guid Id { get; set; }
    public string? Icon { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public List<Guid> ImageFileIds { get; set; } = new List<Guid>();
}
