using ComSci.Application.Dtos.ShortCourses;

namespace ComSci.Application.Abstractions;

public interface IShortCourseDocumentService
{
    Task<byte[]> CreateSignInSheetAsync(Guid shortCourseId, CancellationToken cancellationToken);

    Task<byte[]> CreateCertificatesForCourseAsync(Guid shortCourseId, CancellationToken cancellationToken);

    Task<byte[]> CreateCertificateForRegistrationAsync(Guid shortCourseId, Guid registrationId, CancellationToken cancellationToken);

    Task<byte[]> CreateCertificateForOwnerAsync(ShortCourseCertificateRequest request, CancellationToken cancellationToken);
}
