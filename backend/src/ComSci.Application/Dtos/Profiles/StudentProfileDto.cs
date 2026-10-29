namespace ComSci.Application.Dtos.Profiles;

public class StudentProfileDto
{
    public string StudentCode { get; set; } = string.Empty;
    public short EntryYear { get; set; }
    public string? PreviousSchool { get; set; }
    public string? Address { get; set; }
}
