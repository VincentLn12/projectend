using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Profiles;
using ComSci.Application.Dtos.Users;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ComSci.Infrastructure.Services;

public class UserAdminService : IUserAdminService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUser _currentUser;

    public UserAdminService(AppDbContext dbContext, UserManager<AppUser> userManager, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<AdminUserListItemDto>> GetAllAsync(AdminUserQuery query, CancellationToken cancellationToken)
    {
        IQueryable<AppUser> userQuery = _dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string keyword = query.Search.Trim();
            userQuery = userQuery.Where(user =>
                user.FullName.Contains(keyword)
                || (user.Email != null && user.Email.Contains(keyword)));
        }

        if (query.UserType.HasValue)
        {
            UserType wantedType = query.UserType.Value;
            userQuery = userQuery.Where(user => user.UserType == wantedType);
        }

        int total = await userQuery.CountAsync(cancellationToken);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<AdminUserListItemDto> items = await userQuery
            .OrderBy(user => user.FullName)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(user => new AdminUserListItemDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                UserType = user.UserType,
                StudentCode = user.StudentProfile != null ? user.StudentProfile.StudentCode : null,
                IsLocked = user.LockoutEnd != null && user.LockoutEnd > now,
                CreatedAt = user.CreatedAt
            })
            .ToListAsync(cancellationToken);

        PagedResult<AdminUserListItemDto> result = new PagedResult<AdminUserListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<AdminUserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        AdminUserDto? dto = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new AdminUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Bio = user.Bio,
                ImageFileId = user.ImageFileId,
                UserType = user.UserType,
                IsLocked = user.LockoutEnd != null && user.LockoutEnd > now,
                ShowEmail = user.ShowEmail,
                ShowProfile = user.ShowProfile,
                ConsentAt = user.ConsentAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Student = user.StudentProfile == null ? null : new StudentProfileDto
                {
                    StudentCode = user.StudentProfile.StudentCode,
                    EntryYear = user.StudentProfile.EntryYear,
                    PreviousSchool = user.StudentProfile.PreviousSchool,
                    Address = user.StudentProfile.Address
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw new NotFoundException("User not found.");
        }

        return dto;
    }

    public async Task<AdminUserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        string email = request.Email.Trim();

        AppUser? existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            throw new ConflictException("This email is already used.");
        }

        await ValidateStudentFieldsAsync(request.UserType, request.StudentCode, request.EntryYear, null, cancellationToken);

        AppUser user = new AppUser();
        user.UserName = email;
        user.Email = email;
        user.EmailConfirmed = true;
        user.FullName = request.FullName.Trim();
        user.UserType = request.UserType;

        // สร้างผู้ใช้ บทบาท และโปรไฟล์นักศึกษาให้สำเร็จพร้อมกัน ไม่งั้นยกเลิกทั้งหมด
        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
        {
            IdentityResult created = await _userManager.CreateAsync(user, request.Password);
            ThrowIfFailed(created);

            IdentityResult roleAdded = await _userManager.AddToRoleAsync(user, GetRoleName(request.UserType));
            ThrowIfFailed(roleAdded);

            if (request.UserType == UserType.Student)
            {
                AddStudentProfile(user.Id, request.StudentCode!, request.EntryYear!.Value, request.PreviousSchool, request.Address);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserDto> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        AppUser user = await LoadUserAsync(userId);

        await ValidateStudentFieldsAsync(request.UserType, request.StudentCode, request.EntryYear, userId, cancellationToken);

        bool typeChanged = user.UserType != request.UserType;
        if (typeChanged && user.UserType == UserType.Admin)
        {
            await EnsureNotLastAdminAsync();
        }

        user.FullName = request.FullName.Trim();

        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
        {
            if (typeChanged)
            {
                user.UserType = request.UserType;

                IList<string> currentRoles = await _userManager.GetRolesAsync(user);
                ThrowIfFailed(await _userManager.RemoveFromRolesAsync(user, currentRoles));
                ThrowIfFailed(await _userManager.AddToRoleAsync(user, GetRoleName(request.UserType)));
            }

            StudentProfile? profile = await _dbContext.StudentProfiles
                .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

            if (request.UserType == UserType.Student)
            {
                if (profile == null)
                {
                    AddStudentProfile(userId, request.StudentCode!, request.EntryYear!.Value, request.PreviousSchool, request.Address);
                }
                else
                {
                    profile.StudentCode = request.StudentCode!.Trim();
                    profile.EntryYear = request.EntryYear!.Value;
                    profile.PreviousSchool = request.PreviousSchool?.Trim();
                    profile.Address = request.Address?.Trim();
                }
            }
            else if (profile != null)
            {
                _dbContext.StudentProfiles.Remove(profile);
            }

            ThrowIfFailed(await _userManager.UpdateAsync(user));
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        return await GetByIdAsync(userId, cancellationToken);
    }

    public async Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        AppUser user = await LoadUserAsync(userId);

        // ไม่ใช้ token รีเซ็ตรหัสผ่านของ Identity จึงลบแล้วตั้งใหม่ ถ้าตั้งใหม่ไม่ผ่านให้ย้อนกลับ ไม่ทิ้งบัญชีไว้ไม่มีรหัสผ่าน
        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
        {
            ThrowIfFailed(await _userManager.RemovePasswordAsync(user));
            ThrowIfFailed(await _userManager.AddPasswordAsync(user, request.NewPassword));
            await _userManager.ResetAccessFailedCountAsync(user);
            await RevokeRefreshTokensAsync(userId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async Task SetLockedAsync(Guid userId, bool isLocked, CancellationToken cancellationToken)
    {
        if (isLocked && userId == _currentUser.UserId)
        {
            throw new BadRequestException("You cannot lock your own account.");
        }

        AppUser user = await LoadUserAsync(userId);

        ThrowIfFailed(await _userManager.SetLockoutEnabledAsync(user, true));
        DateTimeOffset? lockoutEnd = isLocked ? DateTimeOffset.MaxValue : null;
        ThrowIfFailed(await _userManager.SetLockoutEndDateAsync(user, lockoutEnd));

        if (isLocked)
        {
            await RevokeRefreshTokensAsync(userId, cancellationToken);
        }
        else
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == _currentUser.UserId)
        {
            throw new BadRequestException("You cannot delete your own account.");
        }

        AppUser user = await LoadUserAsync(userId);

        if (user.UserType == UserType.Admin)
        {
            await EnsureNotLastAdminAsync();
        }

        // เพิกถอน token ก่อนลบ เพราะหลังลบ (soft delete) แถว token จะถูกซ่อนไปด้วย
        await RevokeRefreshTokensAsync(userId, cancellationToken);

        // UserManager.DeleteAsync สั่งลบตามปกติ แล้ว interceptor เปลี่ยนเป็น soft delete
        ThrowIfFailed(await _userManager.DeleteAsync(user));
    }

    private async Task<AppUser> LoadUserAsync(Guid userId)
    {
        AppUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return user;
    }

    private async Task EnsureNotLastAdminAsync()
    {
        IList<AppUser> admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        if (admins.Count <= 1)
        {
            throw new BadRequestException("Cannot remove the last administrator.");
        }
    }

    private async Task ValidateStudentFieldsAsync(
        UserType userType,
        string? studentCode,
        short? entryYear,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        if (userType != UserType.Student)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(studentCode) || !entryYear.HasValue)
        {
            throw new BadRequestException("StudentCode and EntryYear are required for students.");
        }

        string code = studentCode.Trim();
        bool codeInUse = await _dbContext.StudentProfiles.AnyAsync(
            profile => profile.StudentCode == code && profile.UserId != currentUserId,
            cancellationToken);
        if (codeInUse)
        {
            throw new ConflictException("This student code is already used.");
        }
    }

    private void AddStudentProfile(Guid userId, string studentCode, short entryYear, string? previousSchool, string? address)
    {
        StudentProfile profile = new StudentProfile();
        profile.UserId = userId;
        profile.StudentCode = studentCode.Trim();
        profile.EntryYear = entryYear;
        profile.PreviousSchool = previousSchool?.Trim();
        profile.Address = address?.Trim();
        _dbContext.StudentProfiles.Add(profile);
    }

    private async Task RevokeRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        // UPDATE ตรงๆ ไม่โหลดแถว token เข้ามาให้ EF ติดตาม ไม่งั้นการลบผู้ใช้ทีหลังจะชนกับแถวลูกที่ติดตามอยู่
        DateTime now = DateTime.UtcNow;
        await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevokedAt, now), cancellationToken);
    }

    private static string GetRoleName(UserType userType)
    {
        if (userType == UserType.Student)
        {
            return Roles.Student;
        }

        if (userType == UserType.Lecturer)
        {
            return Roles.Lecturer;
        }

        if (userType == UserType.Admin)
        {
            return Roles.Admin;
        }

        return Roles.User;
    }

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        List<string> messages = result.Errors.Select(error => error.Description).ToList();
        throw new BadRequestException(string.Join(" ", messages));
    }
}
