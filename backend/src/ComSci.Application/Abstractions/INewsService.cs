using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;

namespace ComSci.Application.Abstractions;

public interface INewsService
{
    // public
    Task<PagedResult<NewsListItemDto>> GetPublishedAsync(PageQuery query, CancellationToken ct = default);
    /// <exception cref="Exceptions.NotFoundException">Missing, unpublished or not yet due.</exception>
    Task<NewsDetailDto> GetPublishedByIdAsync(Guid id, CancellationToken ct = default);

    // admin
    Task<PagedResult<AdminNewsListItemDto>> GetAllAsync(AdminNewsQuery query, CancellationToken ct = default);
    Task<AdminNewsDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdminNewsDto> CreateAsync(SaveNewsRequest request, CancellationToken ct = default);
    Task<AdminNewsDto> UpdateAsync(Guid id, SaveNewsRequest request, CancellationToken ct = default);
    Task<AdminNewsDto> SetPublishedAsync(Guid id, bool isPublished, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
