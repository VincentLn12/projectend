using ComSci.Application.Abstractions;
using ComSci.Application.Options;
using Microsoft.Extensions.Options;

namespace ComSci.Infrastructure.Files;

public class LocalFileStorage(IOptions<StorageOptions> options) : IFileStorage
{
    private readonly string _root = Path.GetFullPath(options.Value.RootPath);

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken ct = default)
    {
        // The stored name is generated here; nothing from the client reaches the file system.
        var relative = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), Guid.NewGuid().ToString("N") + extension);
        var full = Resolve(relative);

        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var target = new FileStream(full, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(target, ct);
        return relative.Replace('\\', '/');
    }

    public Stream? OpenRead(string path)
    {
        var full = Resolve(path);
        return File.Exists(full) ? new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var full = Resolve(path);
        if (File.Exists(full)) File.Delete(full);
        return Task.CompletedTask;
    }

    /// <summary>Maps a stored relative path to a full path and refuses anything that escapes the root.</summary>
    private string Resolve(string relative)
    {
        var full = Path.GetFullPath(Path.Combine(_root, relative));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");
        return full;
    }
}
