using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// หลักสูตรอบรม (แต่ละหลักสูตรมีหลายรุ่น)
/// </summary>
public class Course : AuditableEntity
{
    /// <summary>ชื่อหลักสูตร</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>รายละเอียด</summary>
    public string? Description { get; set; }

    /// <summary>ผู้สอน (FK, ไม่บังคับ)</summary>
    public Guid? LecturerId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: ผู้สอน</summary>
    public Lecturer? Lecturer { get; set; }

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }

    /// <summary>รูปประกอบหลักสูตร</summary>
    public ICollection<CoursePhoto> Photos { get; set; } = new List<CoursePhoto>();

    /// <summary>รุ่นของหลักสูตร</summary>
    public ICollection<Generation> Generations { get; set; } = new List<Generation>();
}
