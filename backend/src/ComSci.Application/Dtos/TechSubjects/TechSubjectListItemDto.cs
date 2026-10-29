namespace ComSci.Application.Dtos.TechSubjects;

public class TechSubjectListItemDto
{
    public Guid Id { get; set; }
    public string? Icon { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public Guid? MainImageFileId { get; set; }
}
