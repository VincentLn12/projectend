using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Syllabuses;

namespace ComSci.Application.Abstractions;

public interface ISyllabusService
{
    Task<List<SyllabusListItemDto>> GetPublishedAsync(CancellationToken cancellationToken);

    Task<SyllabusDetailDto> GetPublishedByIdAsync(Guid syllabusId, CancellationToken cancellationToken);

    Task<SyllabusDetailDto> GetCurrentAsync(CancellationToken cancellationToken);

    Task<PagedResult<AdminSyllabusListItemDto>> GetAllAsync(AdminSyllabusQuery query, CancellationToken cancellationToken);

    Task<AdminSyllabusDto> GetByIdAsync(Guid syllabusId, CancellationToken cancellationToken);

    Task<AdminSyllabusDto> CreateAsync(SaveSyllabusRequest request, CancellationToken cancellationToken);

    Task<AdminSyllabusDto> UpdateAsync(Guid syllabusId, SaveSyllabusRequest request, CancellationToken cancellationToken);

    Task<AdminSyllabusDto> SetPublishedAsync(Guid syllabusId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteAsync(Guid syllabusId, CancellationToken cancellationToken);
}
