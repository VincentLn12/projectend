using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Auth;

public record LoginRequest(
    [Required, StringLength(256)] string Identifier,   // email, username or student code
    [Required, StringLength(128)] string Password);

public record RegisterRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(200)] string FullName,
    [Required, StringLength(128, MinimumLength = 8)] string Password,
    [Range(typeof(bool), "true", "true", ErrorMessage = "Privacy policy must be accepted.")] bool AcceptPrivacy);

public record RefreshRequest([Required] string RefreshToken);

public record UserInfo(Guid Id, string FullName, string Email, IReadOnlyList<string> Roles);

public record AuthResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken, UserInfo User);
