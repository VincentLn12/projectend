namespace ComSci.Application.Abstractions;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken);

    Stream? OpenRead(string path);

    Task DeleteAsync(string path, CancellationToken cancellationToken);
}
