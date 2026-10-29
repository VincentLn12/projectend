namespace ComSci.Application.Dtos.News;

/// <summary>
/// ไฟล์รูปที่ผูกกับข่าว
/// </summary>
public class NewsFileDto
{
    /// <summary>รหัสไฟล์</summary>
    public Guid FileId { get; set; }

    /// <summary>เป็นรูปหลักหรือไม่</summary>
    public bool IsMain { get; set; }

    /// <summary>ลำดับการแสดงผล</summary>
    public int SortOrder { get; set; }
}
