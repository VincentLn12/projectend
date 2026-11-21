namespace ComSci.Application.Dtos.Users;

public class AdminStudentListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? ImageFileId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public short EntryYear { get; set; }
    public string? PreviousSchool { get; set; }
    public string? Address { get; set; }
    public int JobHistoryCount { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
}
