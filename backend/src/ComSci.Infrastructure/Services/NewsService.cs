using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

/// <summary>
/// จัดการข่าวประชาสัมพันธ์ ทั้งส่วนสาธารณะและส่วนผู้ดูแล
/// กติกา: ข่าวที่คนทั่วไปเห็นต้องเผยแพร่แล้ว (IsPublished) และถึงเวลาเผยแพร่แล้ว (PublishedAt)
/// </summary>
public class NewsService : INewsService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public NewsService(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    // =====================================================================
    // ส่วนสาธารณะ
    // =====================================================================

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

                // รูปหลักก่อน ถ้าไม่มีรูปหลักใช้รูปที่ลำดับแรกสุด
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

    // =====================================================================
    // ส่วนผู้ดูแล
    // =====================================================================

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
        // โหลดเฉพาะข่าว ไม่โหลดรายการรูป: soft delete ไม่แตะแถวรูป และการติดตามรูปไว้จะทำให้ EF สับสนตอนบันทึก
        News news = await LoadNewsAsync(newsId, cancellationToken);

        // สั่งลบตามปกติ แต่ AuditSaveChangesInterceptor จะเปลี่ยนเป็น soft delete (IsDeleted = true) ให้
        _dbContext.News.Remove(news);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // =====================================================================
    // ตัวช่วยภายในคลาส
    // =====================================================================

    /// <summary>
    /// ข่าวที่คนทั่วไปมองเห็นได้: เผยแพร่แล้ว และเวลาเผยแพร่มาถึงแล้ว
    /// </summary>
    private IQueryable<News> GetPublishedNewsQuery()
    {
        DateTime now = DateTime.UtcNow;

        return _dbContext.News
            .AsNoTracking()
            .Where(news => news.IsPublished && news.PublishedAt <= now);
    }

    /// <summary>
    /// ถ้ามีคำค้น ให้กรองเฉพาะข่าวที่หัวข้อมีคำนั้น
    /// </summary>
    private static IQueryable<News> ApplyTitleSearch(IQueryable<News> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string keyword = search.Trim();
        return query.Where(news => news.Title.Contains(keyword));
    }

    /// <summary>
    /// โหลดเฉพาะตัวข่าว (ไม่รวมรายการรูป) เพื่อนำไปแก้สถานะหรือลบ
    /// </summary>
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

    /// <summary>
    /// โหลดข่าวพร้อมรายการรูปที่ผูกอยู่ เพื่อนำไปแก้ไข (ต้องใช้รายการรูปเดิมเพื่อแทนที่ด้วยรายการใหม่)
    /// </summary>
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

    /// <summary>
    /// ตั้งสถานะเผยแพร่ และบันทึกวันเวลาเผยแพร่ครั้งแรก (ถ้าถอนกลับเป็นฉบับร่างแล้วเผยแพร่ใหม่ จะคงวันเดิมไว้)
    /// </summary>
    private static void ApplyPublishedState(News news, bool isPublished)
    {
        news.IsPublished = isPublished;

        if (isPublished && news.PublishedAt == null)
        {
            news.PublishedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// แทนที่รายการไฟล์ของข่าวด้วยรายการใหม่จากคำขอ (ลำดับในรายการคือลำดับแสดงผล)
    /// </summary>
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

        // ลบรายการเดิมทั้งหมดก่อน แล้วเพิ่มรายการใหม่
        // หมายเหตุ: ห้ามเรียก news.Files.Clear() ที่นี่ เพราะจะเป็นการ "ตัด" ความสัมพันธ์ที่บังคับต้องมี
        // (NewsFile ต้องมีข่าวเสมอ) ทำให้ EF ปฏิเสธ ให้สั่งลบแถวเดิมผ่าน RemoveRange อย่างเดียวพอ
        List<NewsFile> oldFiles = news.Files.ToList();
        _dbContext.NewsFiles.RemoveRange(oldFiles);

        // รูปหลัก: ใช้ตามที่ระบุ ถ้าไม่ระบุใช้รูปแรก
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

            // ต้องสั่ง Add ที่ DbSet ตรงๆ ห้ามใช้ news.Files.Add(...)
            // เพราะ NewsFile มีรหัส GUID ตั้งแต่ตอนสร้าง ถ้าเพิ่มผ่านรายการของข่าวที่ EF ติดตามอยู่แล้ว
            // EF จะเข้าใจว่าเป็นแถวเก่าที่มีอยู่ในฐานข้อมูล แล้วสั่ง UPDATE (ซึ่งไม่เจอแถว) แทน INSERT
            _dbContext.NewsFiles.Add(newsFile);
        }
    }
}
