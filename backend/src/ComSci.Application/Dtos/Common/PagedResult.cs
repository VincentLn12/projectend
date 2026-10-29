namespace ComSci.Application.Dtos.Common;

/// <summary>
/// ผลลัพธ์แบบแบ่งหน้า
/// </summary>
public class PagedResult<T>
{
    /// <summary>รายการในหน้านี้</summary>
    public List<T> Items { get; set; } = new List<T>();

    /// <summary>หน้าปัจจุบัน</summary>
    public int Page { get; set; }

    /// <summary>จำนวนรายการต่อหน้า</summary>
    public int PageSize { get; set; }

    /// <summary>จำนวนรายการทั้งหมดที่ตรงเงื่อนไข</summary>
    public int Total { get; set; }
}
