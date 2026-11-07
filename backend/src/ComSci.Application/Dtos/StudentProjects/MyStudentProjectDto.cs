namespace ComSci.Application.Dtos.StudentProjects;

public class MyStudentProjectDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short AcademicYear { get; set; }
    public byte Semester { get; set; }
    public string? CourseName { get; set; }
    public string? Technologies { get; set; }
    public string? TeamMembers { get; set; }
    public Guid? LecturerId { get; set; }
    public Guid? ImageFileId { get; set; }
    public Guid? PdfFileId { get; set; }
    public string? VideoUrl { get; set; }
    public string? WebUrl { get; set; }
    public string? GithubUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
