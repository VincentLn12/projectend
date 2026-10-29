namespace ComSci.Application.Dtos.Auth;

public class RegisterResult
{
    public AuthResponse? Response { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
