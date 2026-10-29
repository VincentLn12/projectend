namespace ComSci.Application.Dtos.News;

/// <summary>
/// รายละเอียดข่าว (หน้าสาธารณะ)
/// </summary>
public class NewsDetailDto
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

    /// <summary>ชื่อผู้เขียน</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>รหัสไฟล์รูปทั้งหมด เรียงตามลำดับ</summary>
    public List<Guid> ImageFileIds { get; set; } = new List<Guid>();
}
