using ComSci.Application.Dtos.Files;

namespace ComSci.Application.Abstractions;

public interface IFileService
{
    Task<FileDto> UploadAsync(Stream content, string originalName, CancellationToken cancellationToken);

    Task<FileContent> OpenAsync(Guid fileId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken);
}
