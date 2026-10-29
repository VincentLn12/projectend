using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;

namespace ComSci.Application.Abstractions;

public interface INewsService
{
    Task<PagedResult<NewsListItemDto>> GetPublishedAsync(PageQuery pageQuery, CancellationToken cancellationToken);

    Task<NewsDetailDto> GetPublishedByIdAsync(Guid newsId, CancellationToken cancellationToken);

    Task<PagedResult<AdminNewsListItemDto>> GetAllAsync(AdminNewsQuery query, CancellationToken cancellationToken);

    Task<AdminNewsDto> GetByIdAsync(Guid newsId, CancellationToken cancellationToken);

    Task<AdminNewsDto> CreateAsync(SaveNewsRequest request, CancellationToken cancellationToken);

    Task<AdminNewsDto> UpdateAsync(Guid newsId, SaveNewsRequest request, CancellationToken cancellationToken);

    Task<AdminNewsDto> SetPublishedAsync(Guid newsId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteAsync(Guid newsId, CancellationToken cancellationToken);
}
