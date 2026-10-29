using ComSci.Application.Dtos.Files;

namespace ComSci.Application.Abstractions;

/// <summary>
/// อัปโหลด เปิด และลบไฟล์ (รูปภาพและ PDF)
/// </summary>
public interface IFileService
{
    /// <summary>
    /// ตรวจและบันทึกไฟล์ที่อัปโหลด
    /// </summary>
    /// <exception cref="Exceptions.BadRequestException">ไฟล์ว่าง ใหญ่เกินกำหนด ชนิดไม่รองรับ หรือนามสกุลไม่ตรงกับเนื้อไฟล์</exception>
    Task<FileDto> UploadAsync(Stream content, string originalName, CancellationToken cancellationToken);

    /// <summary>
    /// เปิดไฟล์เพื่ออ่าน ผู้เรียกต้องปิด Stream ที่ได้
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">ไม่พบไฟล์</exception>
    Task<FileContent> OpenAsync(Guid fileId, CancellationToken cancellationToken);

    /// <summary>
    /// ลบไฟล์ทั้งแถวในฐานข้อมูลและตัวไฟล์จริง
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">ไม่พบไฟล์</exception>
    /// <exception cref="Exceptions.ConflictException">ไฟล์ยังถูกข้อมูลอื่นอ้างอิงอยู่</exception>
    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken);
}
