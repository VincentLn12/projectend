namespace ComSci.Domain.Common;

/// <summary>
/// ข้อมูลที่ต้องเก็บประวัติการสร้าง/แก้ไข และลบแบบ soft delete
/// AuditSaveChangesInterceptor จะเติมค่าให้อัตโนมัติทุกครั้งที่บันทึก
/// </summary>
public interface IAuditable
{
    /// <summary>วันเวลาที่สร้าง (UTC)</summary>
    DateTime CreatedAt { get; set; }

    /// <summary>รหัสผู้สร้าง</summary>
    Guid? CreatedBy { get; set; }

    /// <summary>วันเวลาที่แก้ไขล่าสุด (UTC)</summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>รหัสผู้แก้ไขล่าสุด</summary>
    Guid? UpdatedBy { get; set; }

    /// <summary>true = ถูกลบแบบ soft delete (query ทั่วไปจะไม่เห็นแถวนี้)</summary>
    bool IsDeleted { get; set; }
}
