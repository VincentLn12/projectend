using ComSci.Application.Dtos.Auth;

namespace ComSci.Application.Abstractions;

public interface IAuthService
{
    /// <summary>Returns null when the credentials are wrong or the account is locked.</summary>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<(AuthResponse? Response, IReadOnlyList<string> Errors)> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>Rotates the refresh token. Returns null when it is unknown, expired or already used.</summary>
    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

    Task<UserInfo?> GetUserAsync(Guid userId, CancellationToken ct = default);
}
