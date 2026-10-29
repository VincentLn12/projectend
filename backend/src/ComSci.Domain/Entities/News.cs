using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// ข่าวประชาสัมพันธ์
/// </summary>
public class News : AuditableEntity
{
    /// <summary>หัวข้อข่าว</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>หัวข้อรอง</summary>
    public string? SubTitle { get; set; }

    /// <summary>เนื้อหาข่าว</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>รหัสผู้เขียน (FK)</summary>
    public Guid AuthorId { get; set; }

    /// <summary>ผู้เขียนข่าว</summary>
    public AppUser Author { get; set; } = null!;

    /// <summary>วันเวลาที่เผยแพร่ (UTC) ตั้งค่าตอนเผยแพร่ครั้งแรก</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }

    /// <summary>รูปประกอบข่าว</summary>
    public ICollection<NewsFile> Files { get; set; } = new List<NewsFile>();
}
