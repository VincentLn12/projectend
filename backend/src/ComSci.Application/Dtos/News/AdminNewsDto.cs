namespace ComSci.Application.Dtos.News;

/// <summary>
/// รายละเอียดข่าวทั้งหมดสำหรับผู้ดูแล
/// </summary>
public class AdminNewsDto
{
    /// <summary>รหัสข่าว</summary>
    public Guid Id { get; set; }

    /// <summary>หัวข้อ</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>หัวข้อรอง</summary>
    public string? SubTitle { get; set; }

    /// <summary>เนื้อหา</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>วันเวลาที่เผยแพร่ (UTC)</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>สถานะเผยแพร่</summary>
    public bool IsPublished { get; set; }

    /// <summary>ชื่อผู้เขียน</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>วันที่สร้าง (UTC)</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>วันที่แก้ไขล่าสุด (UTC)</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>ไฟล์รูปที่ผูกกับข่าว</summary>
    public List<NewsFileDto> Files { get; set; } = new List<NewsFileDto>();
}
