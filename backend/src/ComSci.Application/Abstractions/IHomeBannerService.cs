using ComSci.Application.Dtos.HomeBanners;

namespace ComSci.Application.Abstractions;

public interface IHomeBannerService
{
    Task<List<HomeBannerDto>> GetPublishedAsync(CancellationToken cancellationToken);

    Task<List<AdminHomeBannerDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<AdminHomeBannerDto> GetByIdAsync(Guid bannerId, CancellationToken cancellationToken);

    Task<AdminHomeBannerDto> CreateAsync(SaveHomeBannerRequest request, CancellationToken cancellationToken);

    Task<AdminHomeBannerDto> UpdateAsync(Guid bannerId, SaveHomeBannerRequest request, CancellationToken cancellationToken);

    Task<AdminHomeBannerDto> SetPublishedAsync(Guid bannerId, bool isPublished, CancellationToken cancellationToken);

    Task MoveAsync(Guid bannerId, bool moveUp, CancellationToken cancellationToken);

    Task DeleteAsync(Guid bannerId, CancellationToken cancellationToken);
}
