using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Projects;

namespace ComSci.Application.Abstractions;

public interface IProjectService
{
    Task<PagedResult<ProjectListItemDto>> GetPublishedAsync(ProjectQuery query, CancellationToken cancellationToken);

    Task<ProjectDetailDto> GetPublishedByIdAsync(Guid projectId, CancellationToken cancellationToken);

    Task<PagedResult<AdminProjectListItemDto>> GetAllAsync(AdminProjectQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<short>> GetAcademicYearsAsync(CancellationToken cancellationToken);

    Task<AdminProjectDto> GetByIdAsync(Guid projectId, CancellationToken cancellationToken);

    Task<AdminProjectDto> CreateAsync(SaveProjectRequest request, CancellationToken cancellationToken);

    Task<AdminProjectDto> UpdateAsync(Guid projectId, SaveProjectRequest request, CancellationToken cancellationToken);

    Task<AdminProjectDto> SetPublishedAsync(Guid projectId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteAsync(Guid projectId, CancellationToken cancellationToken);
}
