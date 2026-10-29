namespace ComSci.Application.Dtos.News;

/// <summary>
/// ข่าวหนึ่งรายการในหน้ารายการของผู้ดูแล
/// </summary>
public class AdminNewsListItemDto
{
    /// <summary>รหัสข่าว</summary>
    public Guid Id { get; set; }

    /// <summary>หัวข้อ</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>วันเวลาที่เผยแพร่ (UTC)</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>สถานะเผยแพร่</summary>
    public bool IsPublished { get; set; }

    /// <summary>ชื่อผู้เขียน</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>วันที่สร้าง (UTC)</summary>
    public DateTime CreatedAt { get; set; }
}
