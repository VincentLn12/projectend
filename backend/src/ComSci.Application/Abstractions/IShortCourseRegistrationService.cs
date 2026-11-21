using ComSci.Application.Dtos.ShortCourses;

namespace ComSci.Application.Abstractions;

public interface IShortCourseRegistrationService
{
    Task<ShortCourseRegistrationDto> RegisterAsync(Guid shortCourseId, RegisterShortCourseRequest request, CancellationToken cancellationToken);

    Task<List<ShortCourseRegistrationDto>> LookupAsync(LookupShortCourseRegistrationRequest request, CancellationToken cancellationToken);

    Task<ShortCourseRegistrationDto> UpdateAsync(Guid registrationId, UpdateShortCourseRegistrationRequest request, CancellationToken cancellationToken);

    Task CancelAsync(Guid registrationId, CancelShortCourseRegistrationRequest request, CancellationToken cancellationToken);

    Task<List<AdminShortCourseRegistrationDto>> GetByShortCourseAsync(Guid shortCourseId, CancellationToken cancellationToken);

    Task CancelByAdminAsync(Guid shortCourseId, Guid registrationId, CancellationToken cancellationToken);
}
