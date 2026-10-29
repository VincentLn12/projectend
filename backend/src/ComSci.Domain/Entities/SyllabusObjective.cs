namespace ComSci.Domain.Entities;

/// <summary>
/// วัตถุประสงค์ของหลักสูตร (ข้อความ 1 ข้อ ต่อ 1 แถว)
/// </summary>
public class SyllabusObjective
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>หลักสูตร (FK)</summary>
    public Guid SyllabusId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: หลักสูตร</summary>
    public Syllabus Syllabus { get; set; } = null!;

    /// <summary>ข้อความวัตถุประสงค์</summary>
    public string Sentence { get; set; } = string.Empty;

    /// <summary>ลำดับการแสดงผล</summary>
    public int SortOrder { get; set; }
}
