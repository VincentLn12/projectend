using ComSci.Application.Dtos.Auth;

namespace ComSci.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<RegisterResult> RegisterStudentAsync(RegisterStudentRequest request, CancellationToken cancellationToken);

    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken);
}
