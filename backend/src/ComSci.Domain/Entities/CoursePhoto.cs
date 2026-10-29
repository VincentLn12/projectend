namespace ComSci.Domain.Entities;

/// <summary>
/// ตารางเชื่อมหลักสูตรอบรมกับไฟล์รูป
/// </summary>
public class CoursePhoto
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>หลักสูตร (FK)</summary>
    public Guid CourseId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: หลักสูตร</summary>
    public Course Course { get; set; } = null!;

    /// <summary>ไฟล์รูป (FK)</summary>
    public Guid FileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: ไฟล์รูป</summary>
    public FileRecord File { get; set; } = null!;

    /// <summary>true = เป็นรูปหลักของหลักสูตร</summary>
    public bool IsMain { get; set; }
}
