using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.JobHistories;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class JobHistoryService : IJobHistoryService
{
    private readonly AppDbContext _dbContext;

    public JobHistoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<JobHistoryListItemDto>> GetPublishedAsync(PageQuery query, CancellationToken cancellationToken)
    {
        IQueryable<JobHistory> jobQuery = _dbContext.JobHistories
            .AsNoTracking()
            .Where(job => job.IsPublished);
        jobQuery = ApplySearch(jobQuery, query.Search, true);

        int total = await jobQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<JobHistoryListItemDto> items = await jobQuery
            .OrderByDescending(job => job.StartDate)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(job => new JobHistoryListItemDto
            {
                Id = job.Id,
                PersonName = job.User.ShowProfile ? job.User.FullName : PrivacyText.AnonymousName,
                JobName = job.JobName,
                Company = job.Company,
                Position = job.Position,
                StartDate = job.StartDate,
                Description = job.Description
            })
            .ToListAsync(cancellationToken);

        PagedResult<JobHistoryListItemDto> result = new PagedResult<JobHistoryListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<List<JobSummaryItemDto>> GetSummaryAsync(CancellationToken cancellationToken)
    {
        List<JobSummaryItemDto> summary = await _dbContext.JobHistories
            .AsNoTracking()
            .Where(job => job.IsPublished)
            .GroupBy(job => job.JobName)
            .Select(group => new JobSummaryItemDto
            {
                JobName = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.JobName)
            .ToListAsync(cancellationToken);

        return summary;
    }

    public async Task<PagedResult<AdminJobHistoryDto>> GetAllAsync(AdminJobHistoryQuery query, CancellationToken cancellationToken)
    {
        IQueryable<JobHistory> jobQuery = _dbContext.JobHistories.AsNoTracking();
        jobQuery = ApplySearch(jobQuery, query.Search, false);

        if (query.IsPublished.HasValue)
        {
            bool wantedState = query.IsPublished.Value;
            jobQuery = jobQuery.Where(job => job.IsPublished == wantedState);
        }

        if (query.UserId.HasValue)
        {
            Guid wantedUserId = query.UserId.Value;
            jobQuery = jobQuery.Where(job => job.UserId == wantedUserId);
        }

        int total = await jobQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<AdminJobHistoryDto> items = await SelectAdminDtos(jobQuery.OrderByDescending(job => job.CreatedAt))
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        PagedResult<AdminJobHistoryDto> result = new PagedResult<AdminJobHistoryDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<AdminJobHistoryDto> GetByIdAsync(Guid jobHistoryId, CancellationToken cancellationToken)
    {
        IQueryable<JobHistory> jobQuery = _dbContext.JobHistories
            .AsNoTracking()
            .Where(job => job.Id == jobHistoryId);

        AdminJobHistoryDto? dto = await SelectAdminDtos(jobQuery).FirstOrDefaultAsync(cancellationToken);
        if (dto == null)
        {
            throw new NotFoundException("Job history not found.");
        }

        return dto;
    }

    public async Task<AdminJobHistoryDto> CreateAsync(SaveJobHistoryRequest request, CancellationToken cancellationToken)
    {
        await EnsureUserExistsAsync(request.UserId, cancellationToken);

        JobHistory job = new JobHistory();
        CopyRequestToJob(request, job);

        _dbContext.JobHistories.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(job.Id, cancellationToken);
    }

    public async Task<AdminJobHistoryDto> UpdateAsync(Guid jobHistoryId, SaveJobHistoryRequest request, CancellationToken cancellationToken)
    {
        JobHistory job = await LoadJobAsync(jobHistoryId, cancellationToken);
        await EnsureUserExistsAsync(request.UserId, cancellationToken);

        CopyRequestToJob(request, job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(jobHistoryId, cancellationToken);
    }

    public async Task<AdminJobHistoryDto> SetPublishedAsync(Guid jobHistoryId, bool isPublished, CancellationToken cancellationToken)
    {
        JobHistory job = await LoadJobAsync(jobHistoryId, cancellationToken);

        job.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(jobHistoryId, cancellationToken);
    }

    public async Task DeleteAsync(Guid jobHistoryId, CancellationToken cancellationToken)
    {
        JobHistory job = await LoadJobAsync(jobHistoryId, cancellationToken);

        _dbContext.JobHistories.Remove(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<JobHistory> ApplySearch(IQueryable<JobHistory> jobQuery, string? search, bool isPublicView)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return jobQuery;
        }

        string keyword = search.Trim();
        return jobQuery.Where(job =>
            job.JobName.Contains(keyword)
            || (job.Company != null && job.Company.Contains(keyword))
            || ((!isPublicView || job.User.ShowProfile) && job.User.FullName.Contains(keyword)));
    }

    private static IQueryable<AdminJobHistoryDto> SelectAdminDtos(IQueryable<JobHistory> jobQuery)
    {
        return jobQuery.Select(job => new AdminJobHistoryDto
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
        });
    }

    private async Task<JobHistory> LoadJobAsync(Guid jobHistoryId, CancellationToken cancellationToken)
    {
        JobHistory? job = await _dbContext.JobHistories
            .FirstOrDefaultAsync(item => item.Id == jobHistoryId, cancellationToken);
        if (job == null)
        {
            throw new NotFoundException("Job history not found.");
        }

        return job;
    }

    private async Task EnsureUserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        bool exists = await _dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken);
        if (!exists)
        {
            throw new BadRequestException("User does not exist.");
        }
    }

    private static void CopyRequestToJob(SaveJobHistoryRequest request, JobHistory job)
    {
        job.UserId = request.UserId;
        job.JobName = request.JobName.Trim();
        job.Company = request.Company?.Trim();
        job.Position = request.Position?.Trim();
        job.StartDate = request.StartDate;
        job.Description = request.Description;
        job.IsPublished = request.IsPublished;
    }
}
