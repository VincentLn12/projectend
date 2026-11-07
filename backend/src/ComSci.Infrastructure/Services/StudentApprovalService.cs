using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Students;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class StudentApprovalService : IStudentApprovalService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public StudentApprovalService(AppDbContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<List<PendingStudentDto>> GetPendingAsync(CancellationToken cancellationToken)
    {
        List<PendingStudentDto> pending = await _dbContext.StudentProfiles
            .AsNoTracking()
            .Where(profile => !profile.IsApproved)
            .OrderBy(profile => profile.User.CreatedAt)
            .Select(profile => new PendingStudentDto
            {
                UserId = profile.UserId,
                FullName = profile.User.FullName,
                Email = profile.User.Email ?? string.Empty,
                StudentCode = profile.StudentCode,
                EntryYear = profile.EntryYear,
                RegisteredAt = profile.User.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return pending;
    }

    public async Task ApproveAsync(Guid userId, CancellationToken cancellationToken)
    {
        StudentProfile profile = await LoadPendingProfileAsync(userId, cancellationToken);

        AppUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("ไม่พบบัญชีนี้");
        }

        profile.IsApproved = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        bool alreadyStudent = await _userManager.IsInRoleAsync(user, Roles.Student);
        if (!alreadyStudent)
        {
            await _userManager.AddToRoleAsync(user, Roles.Student);
        }
    }

    public async Task RejectAsync(Guid userId, CancellationToken cancellationToken)
    {
        StudentProfile profile = await LoadPendingProfileAsync(userId, cancellationToken);

        AppUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("ไม่พบบัญชีนี้");
        }

        // ลบโปรไฟล์เพื่อคืนรหัสนักศึกษา ให้เจ้าของรหัสตัวจริงสมัครได้ ส่วนบัญชีถูกซ่อนแบบ soft delete
        _dbContext.StudentProfiles.Remove(profile);
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<StudentProfile> LoadPendingProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        StudentProfile? profile = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(item => item.UserId == userId && !item.IsApproved, cancellationToken);
        if (profile == null)
        {
            throw new NotFoundException("ไม่พบคำขอที่รออนุมัติ");
        }

        return profile;
    }
}
