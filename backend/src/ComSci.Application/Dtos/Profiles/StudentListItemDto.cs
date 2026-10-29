namespace ComSci.Application.Dtos.Profiles;

public class StudentListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public short? EntryYear { get; set; }
    public Guid? ImageFileId { get; set; }
    public string? Email { get; set; }
}
