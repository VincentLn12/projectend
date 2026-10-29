using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.HomeBanners;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class HomeBannerService : IHomeBannerService
{
    private readonly AppDbContext _dbContext;

    public HomeBannerService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<HomeBannerDto>> GetPublishedAsync(CancellationToken cancellationToken)
    {
        List<HomeBannerDto> banners = await _dbContext.HomeBanners
            .AsNoTracking()
            .Where(banner => banner.IsPublished)
            .OrderBy(banner => banner.SortOrder)
            .Select(banner => new HomeBannerDto
            {
                Id = banner.Id,
                Title = banner.Title,
                FileId = banner.FileId,
                LinkUrl = banner.LinkUrl
            })
            .ToListAsync(cancellationToken);

        return banners;
    }

    public async Task<List<AdminHomeBannerDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<AdminHomeBannerDto> banners = await SelectAdminDtos(_dbContext.HomeBanners.AsNoTracking())
            .ToListAsync(cancellationToken);

        return banners;
    }

    public async Task<AdminHomeBannerDto> GetByIdAsync(Guid bannerId, CancellationToken cancellationToken)
    {
        IQueryable<HomeBanner> query = _dbContext.HomeBanners
            .AsNoTracking()
            .Where(banner => banner.Id == bannerId);

        AdminHomeBannerDto? dto = await SelectAdminDtos(query).FirstOrDefaultAsync(cancellationToken);
        if (dto == null)
        {
            throw new NotFoundException("Banner not found.");
        }

        return dto;
    }

    public async Task<AdminHomeBannerDto> CreateAsync(SaveHomeBannerRequest request, CancellationToken cancellationToken)
    {
        await EnsureFileExistsAsync(request.FileId, cancellationToken);

        HomeBanner banner = new HomeBanner();
        CopyRequestToBanner(request, banner);

        _dbContext.HomeBanners.Add(banner);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(banner.Id, cancellationToken);
    }

    public async Task<AdminHomeBannerDto> UpdateAsync(Guid bannerId, SaveHomeBannerRequest request, CancellationToken cancellationToken)
    {
        HomeBanner banner = await LoadBannerAsync(bannerId, cancellationToken);
        await EnsureFileExistsAsync(request.FileId, cancellationToken);

        CopyRequestToBanner(request, banner);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(bannerId, cancellationToken);
    }

    public async Task<AdminHomeBannerDto> SetPublishedAsync(Guid bannerId, bool isPublished, CancellationToken cancellationToken)
    {
        HomeBanner banner = await LoadBannerAsync(bannerId, cancellationToken);

        banner.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(bannerId, cancellationToken);
    }

    public async Task DeleteAsync(Guid bannerId, CancellationToken cancellationToken)
    {
        HomeBanner banner = await LoadBannerAsync(bannerId, cancellationToken);

        _dbContext.HomeBanners.Remove(banner);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<AdminHomeBannerDto> SelectAdminDtos(IQueryable<HomeBanner> query)
    {
        return query
            .OrderBy(banner => banner.SortOrder)
            .ThenBy(banner => banner.CreatedAt)
            .Select(banner => new AdminHomeBannerDto
            {
                Id = banner.Id,
                Title = banner.Title,
                FileId = banner.FileId,
                LinkUrl = banner.LinkUrl,
                SortOrder = banner.SortOrder,
                IsPublished = banner.IsPublished,
                CreatedAt = banner.CreatedAt
            });
    }

    private async Task<HomeBanner> LoadBannerAsync(Guid bannerId, CancellationToken cancellationToken)
    {
        HomeBanner? banner = await _dbContext.HomeBanners
            .FirstOrDefaultAsync(item => item.Id == bannerId, cancellationToken);
        if (banner == null)
        {
            throw new NotFoundException("Banner not found.");
        }

        return banner;
    }

    private async Task EnsureFileExistsAsync(Guid fileId, CancellationToken cancellationToken)
    {
        bool exists = await _dbContext.Files.AnyAsync(file => file.Id == fileId, cancellationToken);
        if (!exists)
        {
            throw new BadRequestException("Banner image does not exist.");
        }
    }

    private static void CopyRequestToBanner(SaveHomeBannerRequest request, HomeBanner banner)
    {
        banner.Title = request.Title?.Trim();
        banner.FileId = request.FileId;
        banner.LinkUrl = request.LinkUrl?.Trim();
        banner.SortOrder = request.SortOrder;
        banner.IsPublished = request.IsPublished;
    }
}
