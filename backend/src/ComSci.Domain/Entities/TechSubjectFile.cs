namespace ComSci.Domain.Entities;

/// <summary>
/// ตารางเชื่อมการ์ดวิชากับไฟล์รูป
/// </summary>
public class TechSubjectFile
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>การ์ดวิชา (FK)</summary>
    public Guid TechSubjectId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: การ์ดวิชา</summary>
    public TechSubject TechSubject { get; set; } = null!;

    /// <summary>ไฟล์รูป (FK)</summary>
    public Guid FileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: ไฟล์รูป</summary>
    public FileRecord File { get; set; } = null!;

    /// <summary>ลำดับการแสดงผล</summary>
    public int SortOrder { get; set; }
}
