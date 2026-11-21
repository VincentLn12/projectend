using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.ShortCourses;

namespace ComSci.Application.Abstractions;

public interface IShortCourseService
{
    Task<PagedResult<ShortCourseListItemDto>> GetPublishedAsync(PageQuery query, CancellationToken cancellationToken);

    Task<ShortCourseDetailDto> GetPublishedByIdAsync(Guid shortCourseId, CancellationToken cancellationToken);

    Task<PagedResult<AdminShortCourseListItemDto>> GetAllAsync(AdminShortCourseQuery query, CancellationToken cancellationToken);

    Task<AdminShortCourseDto> GetByIdAsync(Guid shortCourseId, CancellationToken cancellationToken);

    Task<AdminShortCourseDto> CreateAsync(SaveShortCourseRequest request, CancellationToken cancellationToken);

    Task<AdminShortCourseDto> UpdateAsync(Guid shortCourseId, SaveShortCourseRequest request, CancellationToken cancellationToken);

    Task<AdminShortCourseDto> SetPublishedAsync(Guid shortCourseId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteAsync(Guid shortCourseId, CancellationToken cancellationToken);
}
