using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Lecturers;

namespace ComSci.Application.Abstractions;

public interface ILecturerService
{
    Task<List<LecturerDto>> GetPublishedAsync(CancellationToken cancellationToken);

    Task<PagedResult<AdminLecturerDto>> GetAllAsync(AdminLecturerQuery query, CancellationToken cancellationToken);

    Task<AdminLecturerDto> GetByIdAsync(Guid lecturerId, CancellationToken cancellationToken);

    Task<AdminLecturerDto> CreateAsync(SaveLecturerRequest request, CancellationToken cancellationToken);

    Task<AdminLecturerDto> UpdateAsync(Guid lecturerId, SaveLecturerRequest request, CancellationToken cancellationToken);

    Task<AdminLecturerDto> SetPublishedAsync(Guid lecturerId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteAsync(Guid lecturerId, CancellationToken cancellationToken);
}
