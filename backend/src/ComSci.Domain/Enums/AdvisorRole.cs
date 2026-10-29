namespace ComSci.Domain.Enums;

/// <summary>
/// บทบาทอาจารย์ที่ปรึกษาโครงงาน
/// </summary>
public enum AdvisorRole : byte
{
    /// <summary>ที่ปรึกษาหลัก</summary>
    Main = 1,

    /// <summary>ที่ปรึกษาร่วม</summary>
    CoAdvisor = 2
}
