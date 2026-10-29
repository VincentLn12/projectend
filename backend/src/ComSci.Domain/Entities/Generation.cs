using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// รุ่นของหลักสูตรอบรม
/// </summary>
public class Generation : AuditableEntity
{
    /// <summary>หลักสูตร (FK)</summary>
    public Guid CourseId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: หลักสูตร</summary>
    public Course Course { get; set; } = null!;

    /// <summary>ชื่อรุ่น</summary>
    public string? SubTitle { get; set; }

    /// <summary>รายละเอียด</summary>
    public string? Description { get; set; }

    /// <summary>วันเริ่ม</summary>
    public DateTime StartDate { get; set; }

    /// <summary>วันสิ้นสุด</summary>
    public DateTime EndDate { get; set; }

    /// <summary>จำนวนที่นั่ง</summary>
    public int Quantity { get; set; }

    /// <summary>ค่าลงทะเบียน (null = ไม่มีค่าใช้จ่าย)</summary>
    public decimal? Fee { get; set; }

    /// <summary>รูปรุ่น (FK, ไม่บังคับ)</summary>
    public Guid? PhotoFileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: รูปรุ่น</summary>
    public FileRecord? PhotoFile { get; set; }

    /// <summary>true = ยกเลิกรุ่นนี้</summary>
    public bool IsCancelled { get; set; }

    /// <summary>ผู้ลงทะเบียน</summary>
    public ICollection<CourseRegistration> Registrations { get; set; } = new List<CourseRegistration>();

    /// <summary>ความคิดเห็น</summary>
    public ICollection<CourseComment> Comments { get; set; } = new List<CourseComment>();
}
