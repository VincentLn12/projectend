namespace ComSci.Domain.Entities;

/// <summary>
/// ข้อมูลเฉพาะนักศึกษา 1 คนต่อ 1 บัญชีผู้ใช้ (ใช้ UserId เป็นคีย์หลัก)
/// </summary>
public class StudentProfile
{
    /// <summary>รหัสผู้ใช้ (เป็นทั้ง PK และ FK)</summary>
    public Guid UserId { get; set; }

    /// <summary>บัญชีผู้ใช้ของนักศึกษา</summary>
    public AppUser User { get; set; } = null!;

    /// <summary>รหัสนักศึกษา (ห้ามซ้ำ)</summary>
    public string StudentCode { get; set; } = string.Empty;

    /// <summary>ปีการศึกษาที่เข้าเรียน</summary>
    public short EntryYear { get; set; }

    /// <summary>สถานศึกษาเดิม (มัธยม)</summary>
    public string? PreviousSchool { get; set; }

    /// <summary>ที่อยู่ (ไม่แสดงบนหน้าสาธารณะ)</summary>
    public string? Address { get; set; }
}
