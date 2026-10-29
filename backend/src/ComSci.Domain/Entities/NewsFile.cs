namespace ComSci.Domain.Entities;

/// <summary>
/// ตารางเชื่อมข่าวกับไฟล์รูป
/// </summary>
public class NewsFile
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ข่าว (FK)</summary>
    public Guid NewsId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: ข่าว</summary>
    public News News { get; set; } = null!;

    /// <summary>ไฟล์รูป (FK)</summary>
    public Guid FileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: ไฟล์รูป</summary>
    public FileRecord File { get; set; } = null!;

    /// <summary>true = เป็นรูปหลักของข่าว</summary>
    public bool IsMain { get; set; }

    /// <summary>ลำดับการแสดงผล</summary>
    public int SortOrder { get; set; }
}
