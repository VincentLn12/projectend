using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Auth;
using ComSci.Application.Options;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ComSci.Infrastructure.Identity;

public class AuthService(UserManager<AppUser> users, AppDbContext db, IOptions<JwtOptions> options) : IAuthService
{
    private readonly JwtOptions _jwt = options.Value;

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await FindByIdentifierAsync(request.Identifier, ct);
        if (user is null || await users.IsLockedOutAsync(user)) return null;

        if (!await users.CheckPasswordAsync(user, request.Password))
        {
            await users.AccessFailedAsync(user);
            return null;
        }

        await users.ResetAccessFailedCountAsync(user);
        return await IssueAsync(user, ct);
    }

    public async Task<(AuthResponse? Response, IReadOnlyList<string> Errors)> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim();
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            UserType = UserType.General,
            ConsentAt = DateTime.UtcNow,
        };

        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded) return (null, created.Errors.Select(e => e.Description).ToList());

        await users.AddToRoleAsync(user, Roles.User);
        return (await IssueAsync(user, ct), []);
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (stored is null) return null;

        var now = DateTime.UtcNow;
        if (stored.RevokedAt is not null)
        {
            // A used token came back: assume it leaked and end every session of that user.
            await RevokeAllAsync(stored.UserId, now, ct);
            return null;
        }
        if (stored.ExpiresAt <= now) return null;

        stored.RevokedAt = now;
        return await IssueAsync(stored.User, ct); // saves the revoke and the new token together
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, ct);
        if (stored is null) return;

        stored.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<UserInfo?> GetUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        return user is null ? null : await ToInfoAsync(user);
    }

    private async Task<AppUser?> FindByIdentifierAsync(string identifier, CancellationToken ct)
    {
        var id = identifier.Trim();
        if (id.Contains('@')) return await users.FindByEmailAsync(id);

        var byName = await users.FindByNameAsync(id);
        if (byName is not null) return byName;

        var userId = await db.StudentProfiles.Where(s => s.StudentCode == id).Select(s => s.UserId).FirstOrDefaultAsync(ct);
        return userId == Guid.Empty ? null : await users.FindByIdAsync(userId.ToString());
    }

    private async Task<AuthResponse> IssueAsync(AppUser user, CancellationToken ct)
    {
        var info = await ToInfoAsync(user);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, info.Email),
        };
        claims.AddRange(info.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: expires, signingCredentials: credentials);

        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(raw),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
        });
        await db.SaveChangesAsync(ct);

        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(jwt), expires, raw, info);
    }

    private async Task<UserInfo> ToInfoAsync(AppUser user) =>
        new(user.Id, user.FullName, user.Email ?? string.Empty, (await users.GetRolesAsync(user)).ToList());

    private async Task RevokeAllAsync(Guid userId, DateTime now, CancellationToken ct)
    {
        var active = await db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(ct);
        foreach (var t in active) t.RevokedAt = now;
        await db.SaveChangesAsync(ct);
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
