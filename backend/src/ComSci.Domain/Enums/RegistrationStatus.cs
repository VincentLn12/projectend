namespace ComSci.Domain.Enums;

/// <summary>
/// สถานะการลงทะเบียนอบรม
/// </summary>
public enum RegistrationStatus : byte
{
    /// <summary>รอยืนยัน</summary>
    Pending = 1,

    /// <summary>ยืนยันแล้ว</summary>
    Confirmed = 2,

    /// <summary>ยกเลิก</summary>
    Cancelled = 3
}
