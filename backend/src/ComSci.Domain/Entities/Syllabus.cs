using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// หลักสูตรการศึกษา เช่น วท.บ. วิทยาการคอมพิวเตอร์ ปี 2569
/// </summary>
public class Syllabus : AuditableEntity
{
    /// <summary>ชื่อหลักสูตร (ไทย)</summary>
    public string NameTH { get; set; } = string.Empty;

    /// <summary>ชื่อหลักสูตร (อังกฤษ)</summary>
    public string? NameEN { get; set; }

    /// <summary>ปีของหลักสูตร (เช่น 2569)</summary>
    public short Year { get; set; }

    /// <summary>ชื่อปริญญา (ไทย)</summary>
    public string? DegreeTH { get; set; }

    /// <summary>ชื่อปริญญา (อังกฤษ)</summary>
    public string? DegreeEN { get; set; }

    /// <summary>หน่วยกิตรวม (เช่น 121)</summary>
    public short? TotalCredits { get; set; }

    /// <summary>ไฟล์ PDF หลักสูตร (FK, ไม่บังคับ)</summary>
    public Guid? PdfFileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: ไฟล์ PDF หลักสูตร</summary>
    public FileRecord? PdfFile { get; set; }

    /// <summary>true = เป็นหลักสูตรปัจจุบัน</summary>
    public bool IsCurrent { get; set; }

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }

    /// <summary>รายวิชา</summary>
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    /// <summary>วัตถุประสงค์</summary>
    public ICollection<SyllabusObjective> Objectives { get; set; } = new List<SyllabusObjective>();

    /// <summary>อาชีพหลังจบ</summary>
    public ICollection<SyllabusOccupation> Occupations { get; set; } = new List<SyllabusOccupation>();
}
