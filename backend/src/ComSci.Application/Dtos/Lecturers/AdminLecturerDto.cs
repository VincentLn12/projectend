namespace ComSci.Application.Dtos.Lecturers;

public class AdminLecturerDto
{
    public Guid Id { get; set; }
    public string? Prefix { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Expertise { get; set; }
    public string? EducationLevel { get; set; }
    public string? Program { get; set; }
    public string? Contact { get; set; }
    public Guid? ImageFileId { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
