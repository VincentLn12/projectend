using ComSci.Application.Dtos.TechSubjects;

namespace ComSci.Application.Abstractions;

public interface ITechSubjectService
{
    Task<List<TechSubjectListItemDto>> GetPublishedAsync(CancellationToken cancellationToken);

    Task<TechSubjectDetailDto> GetPublishedByIdAsync(Guid subjectId, CancellationToken cancellationToken);

    Task<List<AdminTechSubjectDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<AdminTechSubjectDto> GetByIdAsync(Guid subjectId, CancellationToken cancellationToken);

    Task<AdminTechSubjectDto> CreateAsync(SaveTechSubjectRequest request, CancellationToken cancellationToken);

    Task<AdminTechSubjectDto> UpdateAsync(Guid subjectId, SaveTechSubjectRequest request, CancellationToken cancellationToken);

    Task<AdminTechSubjectDto> SetPublishedAsync(Guid subjectId, bool isPublished, CancellationToken cancellationToken);

    Task MoveAsync(Guid subjectId, bool moveUp, CancellationToken cancellationToken);

    Task DeleteAsync(Guid subjectId, CancellationToken cancellationToken);
}
