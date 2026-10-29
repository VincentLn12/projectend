using ComSci.Domain.Common;
using ComSci.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ComSci.Domain.Entities;

/// <summary>
/// ผู้ใช้ทุกประเภท (ตาราง Users) สืบจาก IdentityUser จึงมี Id, UserName, Email, PasswordHash, LockoutEnd, AccessFailedCount อยู่แล้ว
/// </summary>
public class AppUser : IdentityUser<Guid>, IAuditable
{
    /// <summary>ชื่อ-นามสกุล</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>ประวัติย่อ</summary>
    public string? Bio { get; set; }

    /// <summary>รูปโปรไฟล์ (FK, ไม่บังคับ)</summary>
    public Guid? ImageFileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: รูปโปรไฟล์</summary>
    public FileRecord? ImageFile { get; set; }

    /// <summary>ประเภทผู้ใช้</summary>
    public UserType UserType { get; set; } = UserType.General;

    /// <summary>ยินยอมให้แสดงอีเมลบนหน้าสาธารณะ (PDPA)</summary>
    public bool ShowEmail { get; set; }

    /// <summary>ยินยอมให้แสดงโปรไฟล์บนหน้าสาธารณะ (PDPA)</summary>
    public bool ShowProfile { get; set; }

    /// <summary>วันที่ผู้ใช้ให้ความยินยอมนโยบายความเป็นส่วนตัว</summary>
    public DateTime? ConsentAt { get; set; }

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }

    /// <summary>วันเวลาที่สร้าง (UTC) ระบบเติมให้อัตโนมัติ</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>รหัสผู้สร้าง ระบบเติมให้อัตโนมัติ</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>วันเวลาที่แก้ไขล่าสุด (UTC)</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>รหัสผู้แก้ไขล่าสุด</summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>true = ถูกลบแบบ soft delete (query ทั่วไปจะไม่เห็นแถวนี้)</summary>
    public bool IsDeleted { get; set; }

    /// <summary>ข้อมูลเฉพาะนักศึกษา (ถ้าเป็นนักศึกษา)</summary>
    public StudentProfile? StudentProfile { get; set; }

    /// <summary>ประวัติการทำงาน</summary>
    public ICollection<JobHistory> JobHistories { get; set; } = new List<JobHistory>();

    /// <summary>refresh token ที่ออกให้ผู้ใช้นี้</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
