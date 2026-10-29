namespace ComSci.Domain.Entities;

/// <summary>
/// ข้อมูลไฟล์ที่อัปโหลด (ตาราง Files) ตัวไฟล์จริงเก็บใน IFileStorage
/// </summary>
public class FileRecord
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ชื่อไฟล์เดิมที่ผู้ใช้อัปโหลด (ใช้แสดงผลเท่านั้น)</summary>
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>ตำแหน่งไฟล์ในที่เก็บ (ชื่อที่ระบบสร้างเอง)</summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>ชนิดไฟล์ (MIME) ที่ตรวจจากเนื้อไฟล์</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>ขนาดไฟล์เป็นไบต์</summary>
    public long SizeBytes { get; set; }

    /// <summary>รหัสผู้อัปโหลด</summary>
    public Guid UploadedBy { get; set; }

    /// <summary>วันเวลาที่อัปโหลด (UTC)</summary>
    public DateTime UploadedAt { get; set; }
}
