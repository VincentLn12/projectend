namespace ComSci.Application.Dtos.News;

/// <summary>
/// ข้อมูลสำหรับเปลี่ยนสถานะเผยแพร่ของข่าว
/// </summary>
public class SetPublishedRequest
{
    /// <summary>true = เผยแพร่, false = ถอนกลับเป็นฉบับร่าง</summary>
    public bool IsPublished { get; set; }
}
