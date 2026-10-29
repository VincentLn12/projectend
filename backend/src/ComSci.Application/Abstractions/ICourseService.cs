using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Courses;

namespace ComSci.Application.Abstractions;

public interface ICourseService
{
    Task<PagedResult<CourseListItemDto>> GetPublishedAsync(PageQuery query, CancellationToken cancellationToken);

    Task<CourseDetailDto> GetPublishedByIdAsync(Guid courseId, CancellationToken cancellationToken);

    Task<PagedResult<AdminCourseListItemDto>> GetAllAsync(AdminCourseQuery query, CancellationToken cancellationToken);

    Task<AdminCourseDto> GetByIdAsync(Guid courseId, CancellationToken cancellationToken);

    Task<AdminCourseDto> CreateAsync(SaveCourseRequest request, CancellationToken cancellationToken);

    Task<AdminCourseDto> UpdateAsync(Guid courseId, SaveCourseRequest request, CancellationToken cancellationToken);

    Task<AdminCourseDto> SetPublishedAsync(Guid courseId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteAsync(Guid courseId, CancellationToken cancellationToken);

    Task<GenerationDto> CreateGenerationAsync(Guid courseId, SaveGenerationRequest request, CancellationToken cancellationToken);

    Task<GenerationDto> UpdateGenerationAsync(Guid generationId, SaveGenerationRequest request, CancellationToken cancellationToken);

    Task DeleteGenerationAsync(Guid generationId, CancellationToken cancellationToken);
}
