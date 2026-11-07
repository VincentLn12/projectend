namespace ComSci.Domain.Entities;

public class StudentProfile
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string StudentCode { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;
    public short EntryYear { get; set; }
    public string? PreviousSchool { get; set; }
    public string? Address { get; set; }
}
