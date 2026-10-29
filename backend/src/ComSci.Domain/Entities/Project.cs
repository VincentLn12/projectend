using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// โครงงานวิจัยนักศึกษา
/// </summary>
public class Project : AuditableEntity
{
    /// <summary>ชื่อโครงงาน (ไทย)</summary>
    public string NameTH { get; set; } = string.Empty;

    /// <summary>ชื่อโครงงาน (อังกฤษ)</summary>
    public string? NameEN { get; set; }

    /// <summary>รายละเอียด</summary>
    public string? Description { get; set; }

    /// <summary>ปีการศึกษา (ใช้กรองและทำ index)</summary>
    public short AcademicYear { get; set; }

    /// <summary>แท็กเทคโนโลยี</summary>
    public string? KeyWords { get; set; }

    /// <summary>ลิงก์วิดีโอ</summary>
    public string? VideoUrl { get; set; }

    /// <summary>ลิงก์เว็บของโครงงาน</summary>
    public string? WebUrl { get; set; }

    /// <summary>ลิงก์ GitHub</summary>
    public string? GithubUrl { get; set; }

    /// <summary>รูปโครงงาน (FK, ไม่บังคับ)</summary>
    public Guid? ImageFileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: รูปโครงงาน</summary>
    public FileRecord? ImageFile { get; set; }

    /// <summary>ไฟล์ PDF โครงงาน (FK, ไม่บังคับ)</summary>
    public Guid? PdfFileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: ไฟล์ PDF โครงงาน</summary>
    public FileRecord? PdfFile { get; set; }

    /// <summary>รหัสนักศึกษาเจ้าของโครงงาน (FK)</summary>
    public Guid StudentId { get; set; }

    /// <summary>นักศึกษาเจ้าของโครงงาน</summary>
    public AppUser Student { get; set; } = null!;

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }

    /// <summary>อาจารย์ที่ปรึกษา</summary>
    public ICollection<ProjectAdvisor> Advisors { get; set; } = new List<ProjectAdvisor>();
}
