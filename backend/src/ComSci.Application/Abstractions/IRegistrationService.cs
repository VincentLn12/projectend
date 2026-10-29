using ComSci.Application.Dtos.Courses;

namespace ComSci.Application.Abstractions;

public interface IRegistrationService
{
    Task<MyRegistrationDto> RegisterAsync(Guid generationId, CancellationToken cancellationToken);

    Task CancelMyRegistrationAsync(Guid generationId, CancellationToken cancellationToken);

    Task<List<MyRegistrationDto>> GetMyRegistrationsAsync(CancellationToken cancellationToken);

    Task<List<RegistrationListItemDto>> GetGenerationRegistrationsAsync(Guid generationId, CancellationToken cancellationToken);

    Task CancelRegistrationAsync(Guid generationId, Guid userId, CancellationToken cancellationToken);
}
