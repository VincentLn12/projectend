using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.JobHistories;
using ComSci.Application.Dtos.Profiles;
using ComSci.Application.Dtos.Projects;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUser _currentUser;

    public ProfileService(AppDbContext dbContext, UserManager<AppUser> userManager, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<MyProfileDto> GetMyProfileAsync(CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();
        return await LoadMyProfileAsync(userId, cancellationToken);
    }

    public async Task<MyProfileDto> UpdateMyProfileAsync(UpdateMyProfileRequest request, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();

        AppUser? user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (request.ImageFileId.HasValue)
        {
            bool fileExists = await _dbContext.Files.AnyAsync(file => file.Id == request.ImageFileId.Value, cancellationToken);
            if (!fileExists)
            {
                throw new BadRequestException("Image file does not exist.");
            }
        }

        user.FullName = request.FullName.Trim();
        user.Bio = request.Bio?.Trim();
        user.ImageFileId = request.ImageFileId;
        user.ShowEmail = request.ShowEmail;
        user.ShowProfile = request.ShowProfile;

        // บันทึกวันที่ผู้ใช้ยินยอมให้แสดงข้อมูลครั้งแรก (หลักฐานตาม PDPA)
        if ((request.ShowEmail || request.ShowProfile) && user.ConsentAt == null)
        {
            user.ConsentAt = DateTime.UtcNow;
        }

        // รหัสนักศึกษาและปีที่เข้าเรียนแก้ได้เฉพาะผู้ดูแล ผู้ใช้แก้ได้แค่สถานศึกษาเดิมและที่อยู่
        StudentProfile? student = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (student != null)
        {
            student.PreviousSchool = request.PreviousSchool?.Trim();
            student.Address = request.Address?.Trim();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await LoadMyProfileAsync(userId, cancellationToken);
    }

    public async Task ChangeMyPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();

        AppUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            List<string> messages = result.Errors.Select(error => error.Description).ToList();
            throw new BadRequestException(string.Join(" ", messages));
        }

        // เปลี่ยนรหัสผ่านแล้ว ให้ทุกอุปกรณ์ต้องล็อกอินใหม่
        List<RefreshToken> activeTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<StudentListItemDto>> GetStudentsAsync(StudentQuery query, CancellationToken cancellationToken)
    {
        IQueryable<AppUser> studentQuery = GetPublicStudentsQuery();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string keyword = query.Search.Trim();
            studentQuery = studentQuery.Where(user => user.FullName.Contains(keyword));
        }

        if (query.EntryYear.HasValue)
        {
            short year = query.EntryYear.Value;
            studentQuery = studentQuery.Where(user => user.StudentProfile!.EntryYear == year);
        }

        int total = await studentQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<StudentListItemDto> items = await studentQuery
            .OrderBy(user => user.FullName)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(user => new StudentListItemDto
            {
                Id = user.Id,
                FullName = user.FullName,
                EntryYear = user.StudentProfile != null ? (short?)user.StudentProfile.EntryYear : null,
                ImageFileId = user.ImageFileId,
                Email = user.ShowEmail ? user.Email : null
            })
            .ToListAsync(cancellationToken);

        PagedResult<StudentListItemDto> result = new PagedResult<StudentListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<StudentDetailDto> GetStudentAsync(Guid userId, CancellationToken cancellationToken)
    {
        StudentDetailDto? detail = await GetPublicStudentsQuery()
            .Where(user => user.Id == userId)
            .Select(user => new StudentDetailDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Bio = user.Bio,
                ImageFileId = user.ImageFileId,
                EntryYear = user.StudentProfile != null ? (short?)user.StudentProfile.EntryYear : null,
                Email = user.ShowEmail ? user.Email : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail == null)
        {
            throw new NotFoundException("Student not found.");
        }

        string studentName = detail.FullName;

        detail.Projects = await _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.StudentId == userId && project.IsPublished)
            .OrderByDescending(project => project.AcademicYear)
            .Select(project => new ProjectListItemDto
            {
                Id = project.Id,
                NameTH = project.NameTH,
                NameEN = project.NameEN,
                AcademicYear = project.AcademicYear,
                KeyWords = project.KeyWords,
                ImageFileId = project.ImageFileId,
                StudentName = studentName
            })
            .ToListAsync(cancellationToken);

        detail.Jobs = await _dbContext.JobHistories
            .AsNoTracking()
            .Where(job => job.UserId == userId && job.IsPublished)
            .OrderByDescending(job => job.StartDate)
            .Select(job => new JobHistoryListItemDto
            {
                Id = job.Id,
                PersonName = studentName,
                JobName = job.JobName,
                Company = job.Company,
                Position = job.Position,
                StartDate = job.StartDate,
                Description = job.Description
            })
            .ToListAsync(cancellationToken);

        return detail;
    }

    // นักศึกษาที่เจ้าตัวยินยอมให้แสดงโปรไฟล์ต่อสาธารณะเท่านั้น
    private IQueryable<AppUser> GetPublicStudentsQuery()
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserType == UserType.Student
                && user.ShowProfile
                && user.StudentProfile != null);
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No authenticated user.");
        }

        return _currentUser.UserId.Value;
    }

    private async Task<MyProfileDto> LoadMyProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        MyProfileDto? profile = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new MyProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Bio = user.Bio,
                ImageFileId = user.ImageFileId,
                UserType = user.UserType,
                ShowEmail = user.ShowEmail,
                ShowProfile = user.ShowProfile,
                ConsentAt = user.ConsentAt,
                Student = user.StudentProfile == null ? null : new StudentProfileDto
                {
                    StudentCode = user.StudentProfile.StudentCode,
                    EntryYear = user.StudentProfile.EntryYear,
                    PreviousSchool = user.StudentProfile.PreviousSchool,
                    Address = user.StudentProfile.Address
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("User not found.");
        }

        return profile;
    }
}
