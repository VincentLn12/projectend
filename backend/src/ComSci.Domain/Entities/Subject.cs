namespace ComSci.Domain.Entities;

/// <summary>
/// รายวิชาในหลักสูตร
/// </summary>
public class Subject
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>หลักสูตร (FK)</summary>
    public Guid SyllabusId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: หลักสูตร</summary>
    public Syllabus Syllabus { get; set; } = null!;

    /// <summary>รหัสวิชา</summary>
    public string? Code { get; set; }

    /// <summary>ชื่อวิชา</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>หน่วยกิต</summary>
    public decimal Credit { get; set; }

    /// <summary>หมวดวิชา (ตัวเลขตามข้อมูลเดิม ยังไม่ทราบความหมายของแต่ละค่า)</summary>
    public byte SubjectCategory { get; set; }
}
