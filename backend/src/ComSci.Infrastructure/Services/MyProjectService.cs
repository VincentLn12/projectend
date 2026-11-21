using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Projects;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class MyProjectService : IMyProjectService
{
    private const int MaximumProjectsPerStudent = 20;
    private const int ListPageSize = 100;

    private readonly IProjectService _projectService;
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public MyProjectService(IProjectService projectService, AppDbContext dbContext, ICurrentUser currentUser)
    {
        _projectService = projectService;
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<List<AdminProjectListItemDto>> GetMineAsync(CancellationToken cancellationToken)
    {
        AdminProjectQuery query = new AdminProjectQuery();
        query.StudentId = GetCurrentUserId();
        query.Page = 1;
        query.PageSize = ListPageSize;

        PagedResult<AdminProjectListItemDto> result = await _projectService.GetAllAsync(query, cancellationToken);
        return result.Items;
    }

    public async Task<AdminProjectDto> GetMineByIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await LoadOwnedAsync(projectId, cancellationToken);
    }

    public async Task<AdminProjectDto> CreateMineAsync(SaveMyProjectRequest request, CancellationToken cancellationToken)
    {
        Guid studentId = GetCurrentUserId();

        int existingCount = await _dbContext.Projects.CountAsync(project => project.StudentId == studentId, cancellationToken);
        if (existingCount >= MaximumProjectsPerStudent)
        {
            throw new BadRequestException("เพิ่มโครงงานวิจัยได้ไม่เกิน " + MaximumProjectsPerStudent + " ชิ้น");
        }

        AdminProjectDto created = await _projectService.CreateAsync(ToSaveRequest(request, studentId), cancellationToken);
        await RecordConsentWhenPublishingAsync(studentId, request.IsPublished, cancellationToken);
        return created;
    }

    public async Task<AdminProjectDto> UpdateMineAsync(Guid projectId, SaveMyProjectRequest request, CancellationToken cancellationToken)
    {
        AdminProjectDto owned = await LoadOwnedAsync(projectId, cancellationToken);

        AdminProjectDto updated = await _projectService.UpdateAsync(projectId, ToSaveRequest(request, owned.StudentId), cancellationToken);
        await RecordConsentWhenPublishingAsync(owned.StudentId, request.IsPublished, cancellationToken);
        return updated;
    }

    public async Task<AdminProjectDto> SetMinePublishedAsync(Guid projectId, bool isPublished, CancellationToken cancellationToken)
    {
        AdminProjectDto owned = await LoadOwnedAsync(projectId, cancellationToken);

        AdminProjectDto updated = await _projectService.SetPublishedAsync(projectId, isPublished, cancellationToken);
        await RecordConsentWhenPublishingAsync(owned.StudentId, isPublished, cancellationToken);
        return updated;
    }

    public async Task DeleteMineAsync(Guid projectId, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(projectId, cancellationToken);
        await _projectService.DeleteAsync(projectId, cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No authenticated user.");
        }

        return _currentUser.UserId.Value;
    }

    // นักศึกษาเห็นและแก้ได้เฉพาะโครงงานของตัวเอง ถ้าไม่ใช่เจ้าของตอบว่าไม่พบ ไม่เปิดเผยว่ามีโครงงานนี้อยู่
    private async Task<AdminProjectDto> LoadOwnedAsync(Guid projectId, CancellationToken cancellationToken)
    {
        Guid studentId = GetCurrentUserId();

        AdminProjectDto project = await _projectService.GetByIdAsync(projectId, cancellationToken);
        if (project.StudentId != studentId)
        {
            throw new NotFoundException("ไม่พบโครงงานนี้");
        }

        return project;
    }

    private static SaveProjectRequest ToSaveRequest(SaveMyProjectRequest request, Guid studentId)
    {
        SaveProjectRequest saveRequest = new SaveProjectRequest();
        saveRequest.NameTH = request.NameTH;
        saveRequest.NameEN = request.NameEN;
        saveRequest.Description = request.Description;
        saveRequest.AcademicYear = request.AcademicYear;
        saveRequest.KeyWords = request.KeyWords;
        saveRequest.VideoUrl = request.VideoUrl;
        saveRequest.WebUrl = request.WebUrl;
        saveRequest.GithubUrl = request.GithubUrl;
        saveRequest.ImageFileId = request.ImageFileId;
        saveRequest.PdfFileId = request.PdfFileId;
        saveRequest.StudentId = studentId;
        saveRequest.IsPublished = request.IsPublished;
        saveRequest.Advisors = request.Advisors;
        return saveRequest;
    }

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
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
