using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// อาจารย์ประจำสาขา (รวมข้อมูลจาก Lecturers และ UserLecturer เดิมไว้ที่เดียว)
/// </summary>
public class Lecturer : AuditableEntity
{
    /// <summary>บัญชีผู้ใช้ (FK ไม่บังคับ ใช้เมื่ออาจารย์ล็อกอินได้)</summary>
    public Guid? UserId { get; set; }

    /// <summary>บัญชีผู้ใช้ของอาจารย์</summary>
    public AppUser? User { get; set; }

    /// <summary>คำนำหน้า/ตำแหน่งวิชาการ</summary>
    public string? Prefix { get; set; }

    /// <summary>ชื่อ-นามสกุล</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>ตำแหน่ง</summary>
    public string? Position { get; set; }

    /// <summary>ความเชี่ยวชาญ</summary>
    public string? Expertise { get; set; }

    /// <summary>วุฒิการศึกษา</summary>
    public string? EducationLevel { get; set; }

    /// <summary>สาขา/หลักสูตรที่สังกัด</summary>
    public string? Program { get; set; }

    /// <summary>ช่องทางติดต่อ</summary>
    public string? Contact { get; set; }

    /// <summary>รูปอาจารย์ (FK, ไม่บังคับ)</summary>
    public Guid? ImageFileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: รูปอาจารย์</summary>
    public FileRecord? ImageFile { get; set; }

    /// <summary>ลำดับการแสดงผล</summary>
    public int SortOrder { get; set; }

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }
}
