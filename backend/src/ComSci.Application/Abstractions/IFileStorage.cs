namespace ComSci.Application.Abstractions;

/// <summary>Where file bytes live. The database only keeps the returned relative path.</summary>
public interface IFileStorage
{
    /// <returns>The relative path to store in the database.</returns>
    Task<string> SaveAsync(Stream content, string extension, CancellationToken ct = default);

    /// <returns>null when the file is missing.</returns>
    Stream? OpenRead(string path);

    Task DeleteAsync(string path, CancellationToken ct = default);
}
