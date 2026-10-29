namespace ComSci.Application.Dtos.Projects;

public class ProjectListItemDto
{
    public Guid Id { get; set; }
    public string NameTH { get; set; } = string.Empty;
    public string? NameEN { get; set; }
    public short AcademicYear { get; set; }
    public string? KeyWords { get; set; }
    public Guid? ImageFileId { get; set; }
    public string StudentName { get; set; } = string.Empty;
}
