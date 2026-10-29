using ComSci.Domain.Enums;

namespace ComSci.Domain.Entities;

/// <summary>
/// การลงทะเบียนอบรม 1 คนต่อ 1 รุ่น (คีย์หลักคือ UserId + GenerationId)
/// </summary>
public class CourseRegistration
{
    /// <summary>รหัสผู้ลงทะเบียน (FK, ส่วนหนึ่งของ PK)</summary>
    public Guid UserId { get; set; }

    /// <summary>ผู้ลงทะเบียน</summary>
    public AppUser User { get; set; } = null!;

    /// <summary>รุ่นที่ลงทะเบียน (ส่วนหนึ่งของ PK) (FK)</summary>
    public Guid GenerationId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: รุ่นที่ลงทะเบียน (ส่วนหนึ่งของ PK)</summary>
    public Generation Generation { get; set; } = null!;

    /// <summary>สถานะการลงทะเบียน</summary>
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

    /// <summary>วันเวลาที่ลงทะเบียน (UTC)</summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>วันเวลาที่ยกเลิก (UTC)</summary>
    public DateTime? CancelledAt { get; set; }
}
