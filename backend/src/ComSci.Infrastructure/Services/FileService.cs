using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Files;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Files;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class FileService(AppDbContext db, IFileStorage storage, ICurrentUser currentUser) : IFileService
{
    public async Task<FileDto> UploadAsync(Stream content, string originalName, CancellationToken ct = default)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        if (!content.CanSeek)
        {
            var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, ct);
            buffer.Position = 0;
            content = buffer;
        }

        var length = content.Length;
        if (length == 0) throw new BadRequestException("File is empty.");
        if (length > FileRules.MaxUploadBytes)
            throw new BadRequestException($"File is larger than {FileRules.MaxUploadBytes / 1024 / 1024} MB.");

        var header = new byte[12];
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        content.Position = 0;

        var kind = FileSignature.Detect(header.AsSpan(0, read))
            ?? throw new BadRequestException("Unsupported file type. Allowed: JPG, PNG, GIF, WebP, PDF.");
        if (!kind.Extensions.Contains(Path.GetExtension(originalName).ToLowerInvariant()))
            throw new BadRequestException("File extension does not match its content.");

        var path = await storage.SaveAsync(content, kind.Extensions[0], ct);
        var name = Path.GetFileName(originalName);
        var record = new FileRecord
        {
            OriginalName = name.Length > 260 ? name[..260] : name,
            StoragePath = path,
            ContentType = kind.ContentType,
            SizeBytes = length,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow,
        };

        try
        {
            db.Files.Add(record);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(path, CancellationToken.None); // no orphan bytes without a row
            throw;
        }

        return new FileDto(record.Id, record.OriginalName, record.ContentType, record.SizeBytes, $"/api/files/{record.Id}", record.UploadedAt);
    }

    public async Task<FileContent> OpenAsync(Guid id, CancellationToken ct = default)
    {
        var record = await db.Files.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new NotFoundException("File not found.");
        var stream = storage.OpenRead(record.StoragePath) ?? throw new NotFoundException("File not found.");
        return new FileContent(stream, record.ContentType);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var record = await db.Files.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new NotFoundException("File not found.");

        db.Files.Remove(record);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("File is still in use.");
        }

        await storage.DeleteAsync(record.StoragePath, ct);
    }
}
