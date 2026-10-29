using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

/// <summary>
/// ประวัติการทำงานของศิษย์เก่า
/// </summary>
public class JobHistory : AuditableEntity
{
    /// <summary>รหัสศิษย์เก่า (FK)</summary>
    public Guid UserId { get; set; }

    /// <summary>ศิษย์เก่าเจ้าของประวัติ</summary>
    public AppUser User { get; set; } = null!;

    /// <summary>ชื่องาน</summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>บริษัท</summary>
    public string? Company { get; set; }

    /// <summary>ตำแหน่ง</summary>
    public string? Position { get; set; }

    /// <summary>วันที่เริ่มงาน</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>รายละเอียด</summary>
    public string? Description { get; set; }

    /// <summary>true = แสดงบนหน้าเว็บสาธารณะ</summary>
    public bool IsPublished { get; set; }
}
