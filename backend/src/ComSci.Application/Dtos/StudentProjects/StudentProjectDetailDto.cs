namespace ComSci.Application.Dtos.StudentProjects;

public class StudentProjectDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short AcademicYear { get; set; }
    public byte Semester { get; set; }
    public string? CourseName { get; set; }
    public string? Technologies { get; set; }
    public string? LecturerName { get; set; }
    public Guid? ImageFileId { get; set; }
    public Guid? PdfFileId { get; set; }
    public string? VideoUrl { get; set; }
    public string? WebUrl { get; set; }
    public string? GithubUrl { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
}
