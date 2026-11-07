using ComSci.Application.Dtos.Projects;

namespace ComSci.Application.Abstractions;

public interface IMyProjectService
{
    Task<List<AdminProjectListItemDto>> GetMineAsync(CancellationToken cancellationToken);

    Task<AdminProjectDto> GetMineByIdAsync(Guid projectId, CancellationToken cancellationToken);

    Task<AdminProjectDto> CreateMineAsync(SaveMyProjectRequest request, CancellationToken cancellationToken);

    Task<AdminProjectDto> UpdateMineAsync(Guid projectId, SaveMyProjectRequest request, CancellationToken cancellationToken);

    Task<AdminProjectDto> SetMinePublishedAsync(Guid projectId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteMineAsync(Guid projectId, CancellationToken cancellationToken);
}
