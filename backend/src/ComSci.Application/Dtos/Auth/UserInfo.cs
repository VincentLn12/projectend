namespace ComSci.Application.Dtos.Auth;

public class UserInfo
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public bool PendingStudentApproval { get; set; }
}
