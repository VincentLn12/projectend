using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.News;

/// <summary>
/// ข้อมูลสำหรับสร้างหรือแก้ไขข่าว
/// </summary>
public class SaveNewsRequest
{
    /// <summary>หัวข้อข่าว</summary>
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    /// <summary>หัวข้อรอง</summary>
    [StringLength(500)]
    public string? SubTitle { get; set; }

    /// <summary>เนื้อหาข่าว</summary>
    [Required]
    public string Body { get; set; } = string.Empty;

    /// <summary>true = เผยแพร่ทันที</summary>
    public bool IsPublished { get; set; }

    /// <summary>รหัสไฟล์รูปที่แนบ (อัปโหลดผ่าน POST /api/files ก่อน) ลำดับในรายการคือลำดับแสดงผล</summary>
    public List<Guid>? FileIds { get; set; }

    /// <summary>รหัสไฟล์รูปหลัก ต้องอยู่ใน FileIds (ไม่ระบุ = ใช้รูปแรก)</summary>
    public Guid? MainFileId { get; set; }
}
