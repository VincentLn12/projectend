namespace ComSci.Application.Dtos.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new UserInfo();
}
