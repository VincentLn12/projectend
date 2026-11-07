namespace ComSci.Application.Dtos.Students;

public class PendingStudentDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public short EntryYear { get; set; }
    public DateTime RegisteredAt { get; set; }
}
