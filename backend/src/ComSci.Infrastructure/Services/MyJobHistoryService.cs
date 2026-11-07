using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.JobHistories;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class MyJobHistoryService : IMyJobHistoryService
{
    private const int MaximumJobsPerStudent = 30;

    private readonly IJobHistoryService _jobHistoryService;
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public MyJobHistoryService(IJobHistoryService jobHistoryService, AppDbContext dbContext, ICurrentUser currentUser)
    {
        _jobHistoryService = jobHistoryService;
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<List<AdminJobHistoryDto>> GetMineAsync(CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();

        List<AdminJobHistoryDto> jobs = await _dbContext.JobHistories
            .AsNoTracking()
            .Where(job => job.UserId == userId)
            .OrderByDescending(job => job.StartDate)
            .Select(job => new AdminJobHistoryDto
            {
                Id = job.Id,
                UserId = job.UserId,
                PersonName = job.User.FullName,
                JobName = job.JobName,
                Company = job.Company,
                Position = job.Position,
                StartDate = job.StartDate,
                Description = job.Description,
                IsPublished = job.IsPublished,
                CreatedAt = job.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return jobs;
    }

    public async Task<AdminJobHistoryDto> CreateMineAsync(SaveMyJobHistoryRequest request, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();

        int existingCount = await _dbContext.JobHistories.CountAsync(job => job.UserId == userId, cancellationToken);
        if (existingCount >= MaximumJobsPerStudent)
        {
            throw new BadRequestException("เพิ่มประสบการณ์ทำงานได้ไม่เกิน " + MaximumJobsPerStudent + " รายการ");
        }

        return await _jobHistoryService.CreateAsync(ToSaveRequest(request, userId), cancellationToken);
    }

    public async Task<AdminJobHistoryDto> UpdateMineAsync(Guid jobHistoryId, SaveMyJobHistoryRequest request, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();
        await EnsureOwnedAsync(jobHistoryId, userId, cancellationToken);

        return await _jobHistoryService.UpdateAsync(jobHistoryId, ToSaveRequest(request, userId), cancellationToken);
    }

    public async Task DeleteMineAsync(Guid jobHistoryId, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();
        await EnsureOwnedAsync(jobHistoryId, userId, cancellationToken);

        await _jobHistoryService.DeleteAsync(jobHistoryId, cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No authenticated user.");
        }

        return _currentUser.UserId.Value;
    }

    private async Task EnsureOwnedAsync(Guid jobHistoryId, Guid userId, CancellationToken cancellationToken)
    {
        bool isOwned = await _dbContext.JobHistories.AnyAsync(job => job.Id == jobHistoryId && job.UserId == userId, cancellationToken);
        if (!isOwned)
        {
            throw new NotFoundException("ไม่พบรายการนี้");
        }
    }

    private static SaveJobHistoryRequest ToSaveRequest(SaveMyJobHistoryRequest request, Guid userId)
    {
        SaveJobHistoryRequest saveRequest = new SaveJobHistoryRequest();
        saveRequest.UserId = userId;
        saveRequest.JobName = request.JobName;
        saveRequest.Company = request.Company;
        saveRequest.Position = request.Position;
        saveRequest.StartDate = request.StartDate;
        saveRequest.Description = request.Description;
        saveRequest.IsPublished = request.IsPublished;
        return saveRequest;
    }
}
