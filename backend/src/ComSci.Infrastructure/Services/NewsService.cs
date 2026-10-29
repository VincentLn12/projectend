using System.Linq.Expressions;
using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class NewsService(AppDbContext db, ICurrentUser currentUser) : INewsService
{
    private static readonly Expression<Func<News, AdminNewsDto>> AdminProjection = n => new AdminNewsDto(
        n.Id, n.Title, n.SubTitle, n.Body, n.PublishedAt, n.IsPublished, n.Author.FullName, n.CreatedAt, n.UpdatedAt,
        n.Files.OrderBy(f => f.SortOrder).Select(f => new NewsFileDto(f.FileId, f.IsMain, f.SortOrder)).ToList());

    // ---------- public ----------

    public async Task<PagedResult<NewsListItemDto>> GetPublishedAsync(PageQuery q, CancellationToken ct = default)
    {
        var query = Search(PublishedNews(), q.Search);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.PublishedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(n => new NewsListItemDto(
                n.Id, n.Title, n.SubTitle, n.PublishedAt,
                n.Files.OrderByDescending(f => f.IsMain).ThenBy(f => f.SortOrder).Select(f => (Guid?)f.FileId).FirstOrDefault()))
            .ToListAsync(ct);
        return new PagedResult<NewsListItemDto>(items, q.Page, q.PageSize, total);
    }

    public async Task<NewsDetailDto> GetPublishedByIdAsync(Guid id, CancellationToken ct = default) =>
        await PublishedNews().Where(n => n.Id == id)
            .Select(n => new NewsDetailDto(
                n.Id, n.Title, n.SubTitle, n.Body, n.PublishedAt, n.Author.FullName,
                n.Files.OrderBy(f => f.SortOrder).Select(f => f.FileId).ToList()))
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("News not found.");

    // ---------- admin ----------

    public async Task<PagedResult<AdminNewsListItemDto>> GetAllAsync(AdminNewsQuery q, CancellationToken ct = default)
    {
        var query = Search(db.News.AsNoTracking(), q.Search);
        if (q.IsPublished is { } published) query = query.Where(n => n.IsPublished == published);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(n => new AdminNewsListItemDto(n.Id, n.Title, n.PublishedAt, n.IsPublished, n.Author.FullName, n.CreatedAt))
            .ToListAsync(ct);
        return new PagedResult<AdminNewsListItemDto>(items, q.Page, q.PageSize, total);
    }

    public async Task<AdminNewsDto> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.News.AsNoTracking().Where(n => n.Id == id).Select(AdminProjection).FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("News not found.");

    public async Task<AdminNewsDto> CreateAsync(SaveNewsRequest request, CancellationToken ct = default)
    {
        var news = new News
        {
            Title = request.Title.Trim(),
            SubTitle = request.SubTitle?.Trim(),
            Body = request.Body,
            AuthorId = currentUser.UserId ?? throw new InvalidOperationException("No authenticated user."),
        };
        ApplyPublished(news, request.IsPublished);
        await ReplaceFilesAsync(news, request, ct);

        db.News.Add(news);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(news.Id, ct);
    }

    public async Task<AdminNewsDto> UpdateAsync(Guid id, SaveNewsRequest request, CancellationToken ct = default)
    {
        var news = await LoadForUpdateAsync(id, ct);
        news.Title = request.Title.Trim();
        news.SubTitle = request.SubTitle?.Trim();
        news.Body = request.Body;
        ApplyPublished(news, request.IsPublished);
        await ReplaceFilesAsync(news, request, ct);

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<AdminNewsDto> SetPublishedAsync(Guid id, bool isPublished, CancellationToken ct = default)
    {
        var news = await LoadForUpdateAsync(id, ct);
        ApplyPublished(news, isPublished);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var news = await LoadForUpdateAsync(id, ct);
        db.News.Remove(news); // the audit interceptor turns this into a soft delete
        await db.SaveChangesAsync(ct);
    }

    // ---------- helpers ----------

    private IQueryable<News> PublishedNews()
    {
        var now = DateTime.UtcNow;
        return db.News.AsNoTracking().Where(n => n.IsPublished && n.PublishedAt <= now);
    }

    private static IQueryable<News> Search(IQueryable<News> query, string? search) =>
        string.IsNullOrWhiteSpace(search) ? query : query.Where(n => n.Title.Contains(search.Trim()));

    private async Task<News> LoadForUpdateAsync(Guid id, CancellationToken ct) =>
        await db.News.Include(n => n.Files).FirstOrDefaultAsync(n => n.Id == id, ct)
        ?? throw new NotFoundException("News not found.");

    private static void ApplyPublished(News news, bool isPublished)
    {
        news.IsPublished = isPublished;
        if (isPublished) news.PublishedAt ??= DateTime.UtcNow;
    }

    private async Task ReplaceFilesAsync(News news, SaveNewsRequest request, CancellationToken ct)
    {
        var ids = request.FileIds?.Distinct().ToList() ?? [];
        if (request.MainFileId is { } main && !ids.Contains(main))
            throw new BadRequestException("MainFileId must be one of FileIds.");
        if (ids.Count > 0 && await db.Files.CountAsync(f => ids.Contains(f.Id), ct) != ids.Count)
            throw new BadRequestException("Some files do not exist.");

        db.NewsFiles.RemoveRange(news.Files.ToList());
        news.Files.Clear();

        var mainId = request.MainFileId ?? ids.FirstOrDefault();
        for (var i = 0; i < ids.Count; i++)
            news.Files.Add(new NewsFile { FileId = ids[i], SortOrder = i, IsMain = ids[i] == mainId });
    }
}
