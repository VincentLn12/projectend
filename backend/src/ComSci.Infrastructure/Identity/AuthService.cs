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

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public AuthService(UserManager<AppUser> userManager, AppDbContext dbContext, IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        AppUser? user = await FindUserByIdentifierAsync(request.Identifier, cancellationToken);
        if (user == null)
        {
            return null;
        }

        bool isLockedOut = await _userManager.IsLockedOutAsync(user);
        if (isLockedOut)
        {
            return null;
        }

        bool passwordIsCorrect = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordIsCorrect)
        {
            // นับครั้งที่ผิด ครบกำหนดแล้ว Identity จะล็อกบัญชีให้เอง
            await _userManager.AccessFailedAsync(user);
            return null;
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        string email = request.Email.Trim();

        AppUser user = new AppUser();
        user.UserName = email;
        user.Email = email;
        user.FullName = request.FullName.Trim();
        user.UserType = UserType.General;
        user.ConsentAt = DateTime.UtcNow;

        RegisterResult result = new RegisterResult();

        IdentityResult createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            foreach (IdentityError error in createResult.Errors)
            {
                result.Errors.Add(error.Description);
            }
            return result;
        }

        await _userManager.AddToRoleAsync(user, Roles.User);
        result.Response = await CreateAuthResponseAsync(user, cancellationToken);
        return result;
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string tokenHash = HashToken(refreshToken);

        RefreshToken? storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        if (storedToken == null)
        {
            return null;
        }

        DateTime now = DateTime.UtcNow;

        if (storedToken.RevokedAt != null)
        {
            // token ที่ใช้แล้วถูกนำมาใช้ซ้ำ อาจรั่ว จึงเพิกถอนทุก session ของผู้ใช้นี้
            await RevokeAllTokensOfUserAsync(storedToken.UserId, now, cancellationToken);
            return null;
        }

        if (storedToken.ExpiresAt <= now)
        {
            return null;
        }

        storedToken.RevokedAt = now;
        // หมุน token: ตัวเก่าถูกเพิกถอนที่นี่ ตัวใหม่บันทึกพร้อมกันใน CreateAuthResponseAsync
        return await CreateAuthResponseAsync(storedToken.User, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string tokenHash = HashToken(refreshToken);

        RefreshToken? storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash && token.RevokedAt == null, cancellationToken);
        if (storedToken == null)
        {
            return;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken)
    {
        AppUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        return await CreateUserInfoAsync(user);
    }

    private async Task<AppUser?> FindUserByIdentifierAsync(string identifier, CancellationToken cancellationToken)
    {
        string trimmed = identifier.Trim();

        if (trimmed.Contains('@'))
        {
            return await _userManager.FindByEmailAsync(trimmed);
        }

        AppUser? userByName = await _userManager.FindByNameAsync(trimmed);
        if (userByName != null)
        {
            return userByName;
        }

        Guid studentUserId = await _dbContext.StudentProfiles
            .Where(profile => profile.StudentCode == trimmed)
            .Select(profile => profile.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (studentUserId == Guid.Empty)
        {
            return null;
        }

        return await _userManager.FindByIdAsync(studentUserId.ToString());
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(AppUser user, CancellationToken cancellationToken)
    {
        UserInfo userInfo = await CreateUserInfoAsync(user);

        DateTime accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        string accessToken = CreateAccessToken(user, userInfo, accessTokenExpiresAt);

        string refreshTokenText = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        RefreshToken refreshToken = new RefreshToken();
        refreshToken.UserId = user.Id;
        refreshToken.TokenHash = HashToken(refreshTokenText);
        refreshToken.CreatedAt = DateTime.UtcNow;
        refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        AuthResponse response = new AuthResponse();
        response.AccessToken = accessToken;
        response.ExpiresAt = accessTokenExpiresAt;
        response.RefreshToken = refreshTokenText;
        response.User = userInfo;
        return response;
    }

    private string CreateAccessToken(AppUser user, UserInfo userInfo, DateTime expiresAt)
    {
        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.Name, user.FullName));
        claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));
        foreach (string role in userInfo.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        SigningCredentials credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<UserInfo> CreateUserInfoAsync(AppUser user)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);

        UserInfo userInfo = new UserInfo();
        userInfo.Id = user.Id;
        userInfo.FullName = user.FullName;
        userInfo.Email = user.Email ?? string.Empty;
        userInfo.Roles = roles.ToList();
        return userInfo;
    }

    private async Task RevokeAllTokensOfUserAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        List<RefreshToken> activeTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string HashToken(string token)
    {
        // เก็บเฉพาะค่าแฮช ฐานข้อมูลรั่วแล้ว token ก็ยังใช้ไม่ได้
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
