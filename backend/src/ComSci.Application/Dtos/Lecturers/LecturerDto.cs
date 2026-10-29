namespace ComSci.Application.Dtos.Lecturers;

public class LecturerDto
{
    public Guid Id { get; set; }
    public string? Prefix { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Expertise { get; set; }
    public string? EducationLevel { get; set; }
    public string? Program { get; set; }
    public Guid? ImageFileId { get; set; }
}
