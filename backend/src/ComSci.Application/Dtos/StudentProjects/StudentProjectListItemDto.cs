namespace ComSci.Application.Dtos.StudentProjects;

public class StudentProjectListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public short AcademicYear { get; set; }
    public byte Semester { get; set; }
    public string? CourseName { get; set; }
    public string? Technologies { get; set; }
    public Guid? ImageFileId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
}
