using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// สไลด์รูปหน้าหลัก
/// </summary>
public class HomeBanner : AuditableEntity
{
    /// <summary>ข้อความบนสไลด์</summary>
    public string? Title { get; set; }

    /// <summary>ไฟล์รูปสไลด์ (FK)</summary>
    public Guid FileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: ไฟล์รูปสไลด์</summary>
    public FileRecord File { get; set; } = null!;

    /// <summary>ลิงก์เมื่อคลิก</summary>
    public string? LinkUrl { get; set; }

    /// <summary>ลำดับการแสดงผล</summary>
    public int SortOrder { get; set; }

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }
}
