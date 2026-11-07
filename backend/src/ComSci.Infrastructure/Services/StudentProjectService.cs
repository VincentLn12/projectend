using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.StudentProjects;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class StudentProjectService : IStudentProjectService
{
    private const int MaximumProjectsPerStudent = 100;

    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public StudentProjectService(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<List<MyStudentProjectDto>> GetMineAsync(CancellationToken cancellationToken)
    {
        Guid studentId = GetCurrentUserId();

        List<StudentProject> projects = await _dbContext.StudentProjects
            .AsNoTracking()
            .Where(project => project.StudentId == studentId)
            .OrderByDescending(project => project.AcademicYear)
            .ThenByDescending(project => project.Semester)
            .ThenByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

        return projects.Select(MapToMyDto).ToList();
    }

    public async Task<MyStudentProjectDto> GetMineByIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        StudentProject project = await LoadMineAsync(projectId, cancellationToken);
        return MapToMyDto(project);
    }

    public async Task<MyStudentProjectDto> CreateMineAsync(SaveStudentProjectRequest request, CancellationToken cancellationToken)
    {
        Guid studentId = GetCurrentUserId();

        int existingCount = await _dbContext.StudentProjects.CountAsync(project => project.StudentId == studentId, cancellationToken);
        if (existingCount >= MaximumProjectsPerStudent)
        {
            throw new BadRequestException("เพิ่มโปรเจคได้ไม่เกิน " + MaximumProjectsPerStudent + " ชิ้น");
        }

        await ValidateAsync(request, cancellationToken);

        StudentProject project = new StudentProject();
        project.StudentId = studentId;
        CopyRequestToProject(request, project);

        _dbContext.StudentProjects.Add(project);
        await RecordConsentWhenPublishingAsync(studentId, request.IsPublished, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToMyDto(project);
    }

    public async Task<MyStudentProjectDto> UpdateMineAsync(Guid projectId, SaveStudentProjectRequest request, CancellationToken cancellationToken)
    {
        StudentProject project = await LoadMineAsync(projectId, cancellationToken);
        await ValidateAsync(request, cancellationToken);

        CopyRequestToProject(request, project);
        await RecordConsentWhenPublishingAsync(project.StudentId, request.IsPublished, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToMyDto(project);
    }

    public async Task<MyStudentProjectDto> SetMinePublishedAsync(Guid projectId, bool isPublished, CancellationToken cancellationToken)
    {
        StudentProject project = await LoadMineAsync(projectId, cancellationToken);
        project.IsPublished = isPublished;
        await RecordConsentWhenPublishingAsync(project.StudentId, isPublished, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToMyDto(project);
    }

    public async Task DeleteMineAsync(Guid projectId, CancellationToken cancellationToken)
    {
        StudentProject project = await LoadMineAsync(projectId, cancellationToken);
        _dbContext.StudentProjects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<StudentProjectListItemDto>> GetPublishedAsync(StudentProjectQuery query, CancellationToken cancellationToken)
    {
        IQueryable<StudentProject> projects = _dbContext.StudentProjects
            .AsNoTracking()
            .Where(project => project.IsPublished);

        if (query.AcademicYear.HasValue)
        {
            short wantedYear = query.AcademicYear.Value;
            projects = projects.Where(project => project.AcademicYear == wantedYear);
        }

        if (query.Semester.HasValue)
        {
            byte wantedSemester = query.Semester.Value;
            projects = projects.Where(project => project.Semester == wantedSemester);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            projects = projects.Where(project => project.Title.Contains(term)
                || (project.Technologies != null && project.Technologies.Contains(term))
                || (project.CourseName != null && project.CourseName.Contains(term))
                || project.Student.FullName.Contains(term));
        }

        int total = await projects.CountAsync(cancellationToken);

        List<StudentProjectListItemDto> items = await projects
            .OrderByDescending(project => project.AcademicYear)
            .ThenByDescending(project => project.Semester)
            .ThenByDescending(project => project.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(project => new StudentProjectListItemDto
            {
                Id = project.Id,
                Title = project.Title,
                AcademicYear = project.AcademicYear,
                Semester = project.Semester,
                CourseName = project.CourseName,
                Technologies = project.Technologies,
                ImageFileId = project.ImageFileId,
                StudentId = project.StudentId,
                StudentName = project.Student.FullName
            })
            .ToListAsync(cancellationToken);

        PagedResult<StudentProjectListItemDto> result = new PagedResult<StudentProjectListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<StudentProjectDetailDto> GetPublishedByIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var row = await _dbContext.StudentProjects
            .AsNoTracking()
            .Where(project => project.Id == projectId && project.IsPublished)
            .Select(project => new
            {
                Detail = new StudentProjectDetailDto
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    AcademicYear = project.AcademicYear,
                    Semester = project.Semester,
                    CourseName = project.CourseName,
                    Technologies = project.Technologies,
                    ImageFileId = project.ImageFileId,
                    PdfFileId = project.PdfFileId,
                    VideoUrl = project.VideoUrl,
                    WebUrl = project.WebUrl,
                    GithubUrl = project.GithubUrl,
                    StudentId = project.StudentId,
                    StudentName = project.Student.FullName
                },
                LecturerPrefix = project.Lecturer != null ? project.Lecturer.Prefix : null,
                LecturerName = project.Lecturer != null ? project.Lecturer.FullName : null
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (row == null)
        {
            throw new NotFoundException("ไม่พบโปรเจคนี้");
        }

        if (row.LecturerName != null)
        {
            row.Detail.LecturerName = FormatLecturerName(row.LecturerPrefix, row.LecturerName);
        }

        return row.Detail;
    }

    public async Task<PagedResult<AdminStudentProjectListItemDto>> GetAllAsync(AdminStudentProjectQuery query, CancellationToken cancellationToken)
    {
        IQueryable<StudentProject> projects = _dbContext.StudentProjects.AsNoTracking();

        if (query.IsPublished.HasValue)
        {
            bool wantedState = query.IsPublished.Value;
            projects = projects.Where(project => project.IsPublished == wantedState);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            projects = projects.Where(project => project.Title.Contains(term) || project.Student.FullName.Contains(term));
        }

        int total = await projects.CountAsync(cancellationToken);

        List<AdminStudentProjectListItemDto> items = await projects
            .OrderByDescending(project => project.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(project => new AdminStudentProjectListItemDto
            {
                Id = project.Id,
                Title = project.Title,
                AcademicYear = project.AcademicYear,
                Semester = project.Semester,
                StudentName = project.Student.FullName,
                IsPublished = project.IsPublished,
                CreatedAt = project.CreatedAt
            })
            .ToListAsync(cancellationToken);

        PagedResult<AdminStudentProjectListItemDto> result = new PagedResult<AdminStudentProjectListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task SetPublishedByAdminAsync(Guid projectId, bool isPublished, CancellationToken cancellationToken)
    {
        StudentProject project = await LoadAnyAsync(projectId, cancellationToken);
        project.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByAdminAsync(Guid projectId, CancellationToken cancellationToken)
    {
        StudentProject project = await LoadAnyAsync(projectId, cancellationToken);
        _dbContext.StudentProjects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No authenticated user.");
        }

        return _currentUser.UserId.Value;
    }

    private async Task<StudentProject> LoadMineAsync(Guid projectId, CancellationToken cancellationToken)
    {
        Guid studentId = GetCurrentUserId();

        StudentProject? project = await _dbContext.StudentProjects
            .FirstOrDefaultAsync(item => item.Id == projectId && item.StudentId == studentId, cancellationToken);
        if (project == null)
        {
            throw new NotFoundException("ไม่พบโปรเจคนี้");
        }

        return project;
    }

    private async Task<StudentProject> LoadAnyAsync(Guid projectId, CancellationToken cancellationToken)
    {
        StudentProject? project = await _dbContext.StudentProjects
            .FirstOrDefaultAsync(item => item.Id == projectId, cancellationToken);
        if (project == null)
        {
            throw new NotFoundException("ไม่พบโปรเจคนี้");
        }

        return project;
    }

    // เผยแพร่ครั้งแรกถือว่านักศึกษายินยอมให้แสดงชื่อ จึงบันทึกวันที่ไว้เป็นหลักฐานตาม PDPA เหมือนการเปิดโปรไฟล์
    private async Task RecordConsentWhenPublishingAsync(Guid studentId, bool isPublishing, CancellationToken cancellationToken)
    {
        if (!isPublishing)
        {
            return;
        }

        AppUser? user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == studentId, cancellationToken);
        if (user != null && user.ConsentAt == null)
        {
            user.ConsentAt = DateTime.UtcNow;
        }
    }

    private async Task ValidateAsync(SaveStudentProjectRequest request, CancellationToken cancellationToken)
    {
        if (request.LecturerId.HasValue)
        {
            bool lecturerExists = await _dbContext.Lecturers.AnyAsync(lecturer => lecturer.Id == request.LecturerId.Value, cancellationToken);
            if (!lecturerExists)
            {
                throw new BadRequestException("ไม่พบอาจารย์ที่เลือก");
            }
        }

        await EnsureFileAsync(request.ImageFileId, "image/", "รูปปก", cancellationToken);
        await EnsureFileAsync(request.PdfFileId, "application/pdf", "ไฟล์ PDF", cancellationToken);
    }

    private async Task EnsureFileAsync(Guid? fileId, string contentTypePrefix, string label, CancellationToken cancellationToken)
    {
        if (!fileId.HasValue)
        {
            return;
        }

        FileRecord? file = await _dbContext.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == fileId.Value, cancellationToken);
        if (file == null)
        {
            throw new BadRequestException("ไม่พบไฟล์" + label);
        }

        if (!file.ContentType.StartsWith(contentTypePrefix))
        {
            throw new BadRequestException(label + "ต้องเป็นไฟล์ชนิดที่ถูกต้อง");
        }
    }

    private static void CopyRequestToProject(SaveStudentProjectRequest request, StudentProject project)
    {
        project.Title = request.Title.Trim();
        project.Description = request.Description;
        project.AcademicYear = request.AcademicYear;
        project.Semester = request.Semester;
        project.CourseName = CleanText(request.CourseName);
        project.Technologies = CleanText(request.Technologies);
        project.LecturerId = request.LecturerId;
        project.ImageFileId = request.ImageFileId;
        project.PdfFileId = request.PdfFileId;
        project.VideoUrl = CleanText(request.VideoUrl);
        project.WebUrl = CleanText(request.WebUrl);
        project.GithubUrl = CleanText(request.GithubUrl);
        project.IsPublished = request.IsPublished;
    }

    private static string? CleanText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string FormatLecturerName(string? prefix, string fullName)
    {
        string trimmedPrefix = prefix?.Trim() ?? string.Empty;
        if (trimmedPrefix.Length == 0 || fullName.StartsWith(trimmedPrefix, StringComparison.Ordinal))
        {
            return fullName;
        }

        string separator = trimmedPrefix.EndsWith("ศาสตราจารย์", StringComparison.Ordinal) ? " " : string.Empty;
        return trimmedPrefix + separator + fullName;
    }

    private static MyStudentProjectDto MapToMyDto(StudentProject project)
    {
        MyStudentProjectDto dto = new MyStudentProjectDto();
        dto.Id = project.Id;
        dto.Title = project.Title;
        dto.Description = project.Description;
        dto.AcademicYear = project.AcademicYear;
        dto.Semester = project.Semester;
        dto.CourseName = project.CourseName;
        dto.Technologies = project.Technologies;
        dto.LecturerId = project.LecturerId;
        dto.ImageFileId = project.ImageFileId;
        dto.PdfFileId = project.PdfFileId;
        dto.VideoUrl = project.VideoUrl;
        dto.WebUrl = project.WebUrl;
        dto.GithubUrl = project.GithubUrl;
        dto.IsPublished = project.IsPublished;
        dto.CreatedAt = project.CreatedAt;
        return dto;
    }
}
