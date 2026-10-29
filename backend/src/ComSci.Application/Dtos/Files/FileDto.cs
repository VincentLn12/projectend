namespace ComSci.Application.Dtos.Files;

/// <summary>
/// ข้อมูลไฟล์ที่ส่งกลับหลังอัปโหลด
/// </summary>
public class FileDto
{
    /// <summary>รหัสไฟล์ (ใช้อ้างอิงตอนบันทึกข่าว โครงงาน ฯลฯ)</summary>
    public Guid Id { get; set; }

    /// <summary>ชื่อไฟล์เดิม</summary>
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>ชนิดไฟล์ (MIME)</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>ขนาดไฟล์เป็นไบต์</summary>
    public long SizeBytes { get; set; }

    /// <summary>ที่อยู่สำหรับเปิดไฟล์</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>วันเวลาที่อัปโหลด (UTC)</summary>
    public DateTime UploadedAt { get; set; }
}
