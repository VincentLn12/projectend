using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Common;

/// <summary>
/// พารามิเตอร์แบ่งหน้าและค้นหา (รับจาก query string)
/// </summary>
public class PageQuery
{
    /// <summary>หน้าที่ต้องการ เริ่มที่ 1</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>จำนวนรายการต่อหน้า (1-100)</summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    /// <summary>คำค้นหา</summary>
    [StringLength(200)]
    public string? Search { get; set; }
}
