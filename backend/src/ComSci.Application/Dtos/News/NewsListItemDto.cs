namespace ComSci.Application.Dtos.News;

/// <summary>
/// ข่าวหนึ่งรายการในหน้ารายการ (หน้าสาธารณะ)
/// </summary>
public class NewsListItemDto
{
    /// <summary>รหัสข่าว</summary>
    public Guid Id { get; set; }

    /// <summary>หัวข้อ</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>หัวข้อรอง</summary>
    public string? SubTitle { get; set; }

    /// <summary>วันเวลาที่เผยแพร่ (UTC)</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>รหัสไฟล์รูปหลัก (ดูรูปที่ GET /api/files/{id})</summary>
    public Guid? MainImageFileId { get; set; }
}
