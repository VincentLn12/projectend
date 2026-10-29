using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// ความคิดเห็นในรุ่นอบรม
/// </summary>
public class CourseComment : AuditableEntity
{
    /// <summary>ข้อความ</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>รหัสผู้เขียน (FK)</summary>
    public Guid AuthorId { get; set; }

    /// <summary>ผู้เขียน</summary>
    public AppUser Author { get; set; } = null!;

    /// <summary>รุ่นที่แสดงความคิดเห็น (FK)</summary>
    public Guid GenerationId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: รุ่นที่แสดงความคิดเห็น</summary>
    public Generation Generation { get; set; } = null!;
}
