using ComSci.Domain.Enums;

namespace ComSci.Domain.Entities;

/// <summary>
/// อาจารย์ที่ปรึกษาของโครงงาน (แทนตาราง Consultants เดิมที่เก็บชื่อเป็นข้อความ)
/// </summary>
public class ProjectAdvisor
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>โครงงาน (FK)</summary>
    public Guid ProjectId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: โครงงาน</summary>
    public Project Project { get; set; } = null!;

    /// <summary>อาจารย์ (FK)</summary>
    public Guid LecturerId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: อาจารย์</summary>
    public Lecturer Lecturer { get; set; } = null!;

    /// <summary>บทบาทที่ปรึกษา</summary>
    public AdvisorRole Role { get; set; } = AdvisorRole.Main;
}
