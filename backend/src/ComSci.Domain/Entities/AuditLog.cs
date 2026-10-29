namespace ComSci.Domain.Entities;

/// <summary>
/// บันทึกการแก้ไขข้อมูลของผู้ดูแล (ตารางพร้อมใช้ แต่ยังไม่มีโค้ดเขียนลงตารางนี้)
/// </summary>
public class AuditLog
{
    /// <summary>รหัสหลัก (ตัวเลขเรียงต่อ)</summary>
    public long Id { get; set; }

    /// <summary>ผู้กระทำ</summary>
    public Guid? UserId { get; set; }

    /// <summary>การกระทำ: Create / Update / Delete</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>ชื่อตารางที่ถูกแก้</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>รหัสแถวที่ถูกแก้</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>ค่าก่อน/หลังแก้ไข (JSON)</summary>
    public string? Changes { get; set; }

    /// <summary>วันเวลาที่เกิดเหตุการณ์ (UTC)</summary>
    public DateTime CreatedAt { get; set; }
}
