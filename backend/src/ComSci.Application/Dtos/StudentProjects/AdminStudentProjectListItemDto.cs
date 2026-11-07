namespace ComSci.Application.Dtos.StudentProjects;

public class AdminStudentProjectListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public short AcademicYear { get; set; }
    public byte Semester { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
