using ComSci.Application.Abstractions;
using ComSci.Application.Options;
using Microsoft.Extensions.Options;

namespace ComSci.Infrastructure.Files;

public class LocalFileStorage : IFileStorage
{
    private readonly string _rootFolder;

    public LocalFileStorage(IOptions<StorageOptions> storageOptions)
    {
        _rootFolder = Path.GetFullPath(storageOptions.Value.RootPath);
    }

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        // ชื่อไฟล์สร้างเองทั้งหมด ไม่มีข้อมูลจาก client ปน
        string year = DateTime.UtcNow.ToString("yyyy");
        string month = DateTime.UtcNow.ToString("MM");
        string fileName = Guid.NewGuid().ToString("N") + extension;
        string relativePath = Path.Combine(year, month, fileName);

        string fullPath = ResolveFullPath(relativePath);
        string folder = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(folder);

        using (FileStream target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(target, cancellationToken);
        }

        return relativePath.Replace('\\', '/');
    }

    public Stream? OpenRead(string path)
    {
        string fullPath = ResolveFullPath(path);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        string fullPath = ResolveFullPath(path);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveFullPath(string relativePath)
    {
        string fullPath = Path.GetFullPath(Path.Combine(_rootFolder, relativePath));
        string rootWithSeparator = _rootFolder + Path.DirectorySeparatorChar;

        // กัน path ที่หลุดออกนอกโฟลเดอร์ root
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid storage path.");
        }

        return fullPath;
    }
}
