using ComSci.Application.Dtos.ShortCourses;

namespace ComSci.Application.Abstractions;

public interface IShortCourseRegistrationService
{
    Task<ShortCourseRegistrationDto> RegisterAsync(Guid shortCourseId, RegisterShortCourseRequest request, CancellationToken cancellationToken);

    Task<ShortCourseRegistrationDto> LookupAsync(LookupShortCourseRegistrationRequest request, CancellationToken cancellationToken);

    Task<ShortCourseRegistrationDto> UpdateAsync(string code, UpdateShortCourseRegistrationRequest request, CancellationToken cancellationToken);

    Task CancelAsync(string code, CancelShortCourseRegistrationRequest request, CancellationToken cancellationToken);

    Task<List<AdminShortCourseRegistrationDto>> GetByShortCourseAsync(Guid shortCourseId, CancellationToken cancellationToken);

    Task CancelByAdminAsync(Guid shortCourseId, Guid registrationId, CancellationToken cancellationToken);
}
