using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class NewsService : INewsService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public NewsService(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<NewsListItemDto>> GetPublishedAsync(PageQuery pageQuery, CancellationToken cancellationToken)
    {
        IQueryable<News> query = GetPublishedNewsQuery();
        query = ApplyTitleSearch(query, pageQuery.Search);

        int total = await query.CountAsync(cancellationToken);

        int itemsToSkip = (pageQuery.Page - 1) * pageQuery.PageSize;
        List<NewsListItemDto> items = await query
            .OrderByDescending(news => news.PublishedAt)
            .Skip(itemsToSkip)
            .Take(pageQuery.PageSize)
            .Select(news => new NewsListItemDto
            {
                Id = news.Id,
                Title = news.Title,
                SubTitle = news.SubTitle,
                PublishedAt = news.PublishedAt,

                MainImageFileId = news.Files
                    .OrderByDescending(file => file.IsMain)
                    .ThenBy(file => file.SortOrder)
                    .Select(file => (Guid?)file.FileId)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        PagedResult<NewsListItemDto> result = new PagedResult<NewsListItemDto>();
        result.Items = items;
        result.Page = pageQuery.Page;
        result.PageSize = pageQuery.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<NewsDetailDto> GetPublishedByIdAsync(Guid newsId, CancellationToken cancellationToken)
    {
        NewsDetailDto? detail = await GetPublishedNewsQuery()
            .Where(news => news.Id == newsId)
            .Select(news => new NewsDetailDto
            {
                Id = news.Id,
                Title = news.Title,
                SubTitle = news.SubTitle,
                Body = news.Body,
                PublishedAt = news.PublishedAt,
                AuthorName = news.Author.FullName,
                ImageFileIds = news.Files
                    .OrderBy(file => file.SortOrder)
                    .Select(file => file.FileId)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail == null)
        {
            throw new NotFoundException("News not found.");
        }

        return detail;
    }

    public async Task<PagedResult<AdminNewsListItemDto>> GetAllAsync(AdminNewsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<News> newsQuery = _dbContext.News.AsNoTracking();
        newsQuery = ApplyTitleSearch(newsQuery, query.Search);

        if (query.IsPublished.HasValue)
        {
            bool wantedState = query.IsPublished.Value;
            newsQuery = newsQuery.Where(news => news.IsPublished == wantedState);
        }

        int total = await newsQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<AdminNewsListItemDto> items = await newsQuery
            .OrderByDescending(news => news.CreatedAt)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(news => new AdminNewsListItemDto
            {
                Id = news.Id,
                Title = news.Title,
                PublishedAt = news.PublishedAt,
                IsPublished = news.IsPublished,
                AuthorName = news.Author.FullName,
                CreatedAt = news.CreatedAt
            })
            .ToListAsync(cancellationToken);

        PagedResult<AdminNewsListItemDto> result = new PagedResult<AdminNewsListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<AdminNewsDto> GetByIdAsync(Guid newsId, CancellationToken cancellationToken)
    {
        AdminNewsDto? dto = await _dbContext.News
            .AsNoTracking()
            .Where(news => news.Id == newsId)
            .Select(news => new AdminNewsDto
            {
                Id = news.Id,
                Title = news.Title,
                SubTitle = news.SubTitle,
                Body = news.Body,
                PublishedAt = news.PublishedAt,
                IsPublished = news.IsPublished,
                AuthorName = news.Author.FullName,
                CreatedAt = news.CreatedAt,
                UpdatedAt = news.UpdatedAt,
                Files = news.Files
                    .OrderBy(file => file.SortOrder)
                    .Select(file => new NewsFileDto
                    {
                        FileId = file.FileId,
                        IsMain = file.IsMain,
                        SortOrder = file.SortOrder
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw new NotFoundException("News not found.");
        }

        return dto;
    }

    public async Task<AdminNewsDto> CreateAsync(SaveNewsRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No authenticated user.");
        }

        News news = new News();
        news.Title = request.Title.Trim();
        news.SubTitle = request.SubTitle?.Trim();
        news.Body = request.Body;
        news.AuthorId = _currentUser.UserId.Value;
        ApplyPublishedState(news, request.IsPublished);
        await ReplaceFilesAsync(news, request, cancellationToken);

        _dbContext.News.Add(news);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(news.Id, cancellationToken);
    }

    public async Task<AdminNewsDto> UpdateAsync(Guid newsId, SaveNewsRequest request, CancellationToken cancellationToken)
    {
        News news = await LoadNewsWithFilesForUpdateAsync(newsId, cancellationToken);

        news.Title = request.Title.Trim();
        news.SubTitle = request.SubTitle?.Trim();
        news.Body = request.Body;
        ApplyPublishedState(news, request.IsPublished);
        await ReplaceFilesAsync(news, request, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(newsId, cancellationToken);
    }

    public async Task<AdminNewsDto> SetPublishedAsync(Guid newsId, bool isPublished, CancellationToken cancellationToken)
    {
        News news = await LoadNewsAsync(newsId, cancellationToken);

        ApplyPublishedState(news, isPublished);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(newsId, cancellationToken);
    }

    public async Task DeleteAsync(Guid newsId, CancellationToken cancellationToken)
    {
        News news = await LoadNewsAsync(newsId, cancellationToken);

        // โหลดเฉพาะข่าว ไม่รวมรูป ไม่งั้น soft delete จะชนกับรูปที่ EF ติดตามอยู่
        // AuditSaveChangesInterceptor เปลี่ยนคำสั่งลบนี้เป็น soft delete ให้
        _dbContext.News.Remove(news);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<News> GetPublishedNewsQuery()
    {
        // ข่าวที่คนทั่วไปเห็น: เผยแพร่แล้ว และถึงเวลาเผยแพร่แล้ว
        DateTime now = DateTime.UtcNow;

        return _dbContext.News
            .AsNoTracking()
            .Where(news => news.IsPublished && news.PublishedAt <= now);
    }

    private static IQueryable<News> ApplyTitleSearch(IQueryable<News> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string keyword = search.Trim();
        return query.Where(news => news.Title.Contains(keyword));
    }

    private async Task<News> LoadNewsAsync(Guid newsId, CancellationToken cancellationToken)
    {
        News? news = await _dbContext.News
            .FirstOrDefaultAsync(item => item.Id == newsId, cancellationToken);

        if (news == null)
        {
            throw new NotFoundException("News not found.");
        }

        return news;
    }

    private async Task<News> LoadNewsWithFilesForUpdateAsync(Guid newsId, CancellationToken cancellationToken)
    {
        News? news = await _dbContext.News
            .Include(item => item.Files)
            .FirstOrDefaultAsync(item => item.Id == newsId, cancellationToken);

        if (news == null)
        {
            throw new NotFoundException("News not found.");
        }

        return news;
    }

    private static void ApplyPublishedState(News news, bool isPublished)
    {
        news.IsPublished = isPublished;

        // คงวันเผยแพร่ครั้งแรกไว้ แม้ถอนเป็นฉบับร่างแล้วเผยแพร่ใหม่
        if (isPublished && news.PublishedAt == null)
        {
            news.PublishedAt = DateTime.UtcNow;
        }
    }

    private async Task ReplaceFilesAsync(News news, SaveNewsRequest request, CancellationToken cancellationToken)
    {
        List<Guid> fileIds = new List<Guid>();
        if (request.FileIds != null)
        {
            fileIds = request.FileIds.Distinct().ToList();
        }

        if (request.MainFileId.HasValue && !fileIds.Contains(request.MainFileId.Value))
        {
            throw new BadRequestException("MainFileId must be one of FileIds.");
        }

        if (fileIds.Count > 0)
        {
            int existingCount = await _dbContext.Files.CountAsync(file => fileIds.Contains(file.Id), cancellationToken);
            if (existingCount != fileIds.Count)
            {
                throw new BadRequestException("Some files do not exist.");
            }
        }

        List<NewsFile> oldFiles = news.Files.ToList();
        // ห้ามเรียก news.Files.Clear(): เป็นการตัดความสัมพันธ์ที่บังคับ EF จะ error
        _dbContext.NewsFiles.RemoveRange(oldFiles);

        Guid mainFileId = Guid.Empty;
        if (request.MainFileId.HasValue)
        {
            mainFileId = request.MainFileId.Value;
        }
        else if (fileIds.Count > 0)
        {
            mainFileId = fileIds[0];
        }

        for (int index = 0; index < fileIds.Count; index++)
        {
            NewsFile newsFile = new NewsFile();
            newsFile.NewsId = news.Id;
            newsFile.FileId = fileIds[index];
            newsFile.SortOrder = index;
            newsFile.IsMain = fileIds[index] == mainFileId;

            // ต้อง Add ที่ DbSet ตรงๆ ไม่ใช้ news.Files.Add: NewsFile มี GUID ตั้งแต่สร้าง
            // EF จะเข้าใจว่าเป็นแถวเก่าแล้วสั่ง UPDATE แทน INSERT
            _dbContext.NewsFiles.Add(newsFile);
        }
    }
}
