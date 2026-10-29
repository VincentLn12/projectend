namespace ComSci.Application.Dtos.Projects;

public class ProjectDetailDto
{
    public Guid Id { get; set; }
    public string NameTH { get; set; } = string.Empty;
    public string? NameEN { get; set; }
    public string? Description { get; set; }
    public short AcademicYear { get; set; }
    public string? KeyWords { get; set; }
    public string? VideoUrl { get; set; }
    public string? WebUrl { get; set; }
    public string? GithubUrl { get; set; }
    public Guid? ImageFileId { get; set; }
    public Guid? PdfFileId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<ProjectAdvisorDto> Advisors { get; set; } = new List<ProjectAdvisorDto>();
}
