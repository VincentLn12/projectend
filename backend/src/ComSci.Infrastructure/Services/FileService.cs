using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Files;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Files;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

/// <summary>
/// อัปโหลด เปิด และลบไฟล์ ตรวจความถูกต้องของไฟล์ก่อนบันทึกทุกครั้ง
/// </summary>
public class FileService : IFileService
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUser _currentUser;

    public FileService(AppDbContext dbContext, IFileStorage fileStorage, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<FileDto> UploadAsync(Stream content, string originalName, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No authenticated user.");
        }
        Guid userId = _currentUser.UserId.Value;

        // ต้องอ่านย้อนกลับได้ เพราะจะอ่านต้นไฟล์เพื่อตรวจชนิดก่อน แล้วค่อยอ่านใหม่ตอนบันทึก
        Stream seekableContent = content;
        if (!seekableContent.CanSeek)
        {
            MemoryStream buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            buffer.Position = 0;
            seekableContent = buffer;
        }

        // ขั้นที่ 1: ตรวจขนาด
        long length = seekableContent.Length;
        if (length == 0)
        {
            throw new BadRequestException("File is empty.");
        }
        if (length > FileRules.MaxUploadBytes)
        {
            long maxMegabytes = FileRules.MaxUploadBytes / 1024 / 1024;
            throw new BadRequestException("File is larger than " + maxMegabytes + " MB.");
        }

        // ขั้นที่ 2: ตรวจชนิดจากเนื้อไฟล์ ไม่เชื่อ Content-Type หรือนามสกุลที่ client ส่งมา
        byte[] header = new byte[12];
        int headerLength = await seekableContent.ReadAtLeastAsync(header, header.Length, false, cancellationToken);
        seekableContent.Position = 0;

        FileKind? kind = FileSignature.Detect(header, headerLength);
        if (kind == null)
        {
            throw new BadRequestException("Unsupported file type. Allowed: JPG, PNG, GIF, WebP, PDF.");
        }

        // ขั้นที่ 3: นามสกุลต้องตรงกับเนื้อไฟล์ (กันการเปลี่ยนนามสกุลหลอก)
        string extension = Path.GetExtension(originalName).ToLowerInvariant();
        if (!kind.Extensions.Contains(extension))
        {
            throw new BadRequestException("File extension does not match its content.");
        }

        // ขั้นที่ 4: เก็บไฟล์จริง แล้วจึงบันทึกแถวในฐานข้อมูล
        string storedPath = await _fileStorage.SaveAsync(seekableContent, kind.Extensions[0], cancellationToken);

        string displayName = Path.GetFileName(originalName);
        if (displayName.Length > 260)
        {
            displayName = displayName.Substring(0, 260);
        }

        FileRecord record = new FileRecord();
        record.OriginalName = displayName;
        record.StoragePath = storedPath;
        record.ContentType = kind.ContentType;
        record.SizeBytes = length;
        record.UploadedBy = userId;
        record.UploadedAt = DateTime.UtcNow;

        try
        {
            _dbContext.Files.Add(record);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // บันทึกฐานข้อมูลไม่สำเร็จ ให้ลบไฟล์ที่เพิ่งเขียนทิ้ง ไม่ให้มีไฟล์กำพร้า
            await _fileStorage.DeleteAsync(storedPath, CancellationToken.None);
            throw;
        }

        FileDto result = new FileDto();
        result.Id = record.Id;
        result.OriginalName = record.OriginalName;
        result.ContentType = record.ContentType;
        result.SizeBytes = record.SizeBytes;
        result.Url = "/api/files/" + record.Id;
        result.UploadedAt = record.UploadedAt;
        return result;
    }

    public async Task<FileContent> OpenAsync(Guid fileId, CancellationToken cancellationToken)
    {
        FileRecord? record = await _dbContext.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(file => file.Id == fileId, cancellationToken);
        if (record == null)
        {
            throw new NotFoundException("File not found.");
        }

        Stream? stream = _fileStorage.OpenRead(record.StoragePath);
        if (stream == null)
        {
            // มีแถวในฐานข้อมูลแต่ไฟล์จริงหายไป
            throw new NotFoundException("File not found.");
        }

        FileContent result = new FileContent();
        result.Content = stream;
        result.ContentType = record.ContentType;
        return result;
    }

    public async Task DeleteAsync(Guid fileId, CancellationToken cancellationToken)
    {
        FileRecord? record = await _dbContext.Files
            .FirstOrDefaultAsync(file => file.Id == fileId, cancellationToken);
        if (record == null)
        {
            throw new NotFoundException("File not found.");
        }

        _dbContext.Files.Remove(record);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // ฐานข้อมูลไม่ยอมลบ เพราะมี FK จากข่าว โครงงาน ฯลฯ อ้างถึงไฟล์นี้อยู่
            throw new ConflictException("File is still in use.");
        }

        // ลบไฟล์จริงหลังจากลบแถวสำเร็จแล้วเท่านั้น
        await _fileStorage.DeleteAsync(record.StoragePath, cancellationToken);
    }
}
