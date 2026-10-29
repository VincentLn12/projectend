using ComSci.Application.Dtos.Files;

namespace ComSci.Application.Abstractions;

public interface IFileService
{
    /// <exception cref="Exceptions.BadRequestException">Empty, too large, unsupported type, or extension and content disagree.</exception>
    Task<FileDto> UploadAsync(Stream content, string originalName, CancellationToken ct = default);

    /// <exception cref="Exceptions.NotFoundException">No such file.</exception>
    Task<FileContent> OpenAsync(Guid id, CancellationToken ct = default);

    /// <exception cref="Exceptions.ConflictException">The file is still referenced by other data.</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
