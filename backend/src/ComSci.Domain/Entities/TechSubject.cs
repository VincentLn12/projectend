using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// การ์ด "วิทย์คอมเราเรียนอะไร" (Flutter, Database, React ฯลฯ)
/// </summary>
public class TechSubject : AuditableEntity
{
    /// <summary>ชื่อไอคอน</summary>
    public string? Icon { get; set; }

    /// <summary>ชื่อหัวข้อ</summary>
    public string SubjectName { get; set; } = string.Empty;

    /// <summary>คำอธิบายสั้น</summary>
    public string? SubTitle { get; set; }

    /// <summary>รายละเอียด</summary>
    public string? Description { get; set; }

    /// <summary>ลำดับการแสดงผล</summary>
    public int SortOrder { get; set; }

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }

    /// <summary>รูปประกอบ</summary>
    public ICollection<TechSubjectFile> Files { get; set; } = new List<TechSubjectFile>();
}
