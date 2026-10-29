namespace ComSci.Domain.Common;

/// <summary>
/// คลาสแม่ของตารางเนื้อหาหลัก มีรหัส GUID และฟิลด์ audit ครบ
/// </summary>
public class AuditableEntity : IAuditable
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

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
}
