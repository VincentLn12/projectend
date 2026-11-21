namespace ComSci.Application.Dtos.Projects;

public class AdminProjectListItemDto
{
    public Guid Id { get; set; }
    public string NameTH { get; set; } = string.Empty;
    public short AcademicYear { get; set; }
    public Guid? ImageFileId { get; set; }
    public string? KeyWords { get; set; }
    public string? VideoUrl { get; set; }
    public string? WebUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
