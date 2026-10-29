using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.News;

/// <summary>
/// พารามิเตอร์ค้นหาข่าวฝั่งผู้ดูแล (เพิ่มตัวกรองสถานะเผยแพร่)
/// </summary>
public class AdminNewsQuery : PageQuery
{
    /// <summary>กรองตามสถานะ: true = เผยแพร่แล้ว, false = ฉบับร่าง, ไม่ส่ง = ทั้งหมด</summary>
    public bool? IsPublished { get; set; }
}
