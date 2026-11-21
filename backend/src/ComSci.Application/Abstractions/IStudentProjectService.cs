using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.StudentProjects;

namespace ComSci.Application.Abstractions;

public interface IStudentProjectService
{
    Task<List<MyStudentProjectDto>> GetMineAsync(CancellationToken cancellationToken);

    Task<MyStudentProjectDto> GetMineByIdAsync(Guid projectId, CancellationToken cancellationToken);

    Task<MyStudentProjectDto> CreateMineAsync(SaveStudentProjectRequest request, CancellationToken cancellationToken);

    Task<MyStudentProjectDto> UpdateMineAsync(Guid projectId, SaveStudentProjectRequest request, CancellationToken cancellationToken);

    Task<MyStudentProjectDto> SetMinePublishedAsync(Guid projectId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteMineAsync(Guid projectId, CancellationToken cancellationToken);

    Task<PagedResult<StudentProjectListItemDto>> GetPublishedAsync(StudentProjectQuery query, CancellationToken cancellationToken);

    Task<StudentProjectDetailDto> GetPublishedByIdAsync(Guid projectId, CancellationToken cancellationToken);

    Task<PagedResult<AdminStudentProjectListItemDto>> GetAllAsync(AdminStudentProjectQuery query, CancellationToken cancellationToken);

    Task SetPublishedByAdminAsync(Guid projectId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteByAdminAsync(Guid projectId, CancellationToken cancellationToken);
}
