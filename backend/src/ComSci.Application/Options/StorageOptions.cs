namespace ComSci.Application.Options;

/// <summary>
/// ค่าตั้งค่าที่เก็บไฟล์ อ่านจาก section "Storage" ใน appsettings
/// </summary>
public class StorageOptions
{
    /// <summary>ชื่อ section ใน appsettings</summary>
    public const string SectionName = "Storage";

    /// <summary>
    /// โฟลเดอร์เก็บไฟล์ที่อัปโหลด ถ้าเป็น path แบบ relative จะอิงจากโฟลเดอร์ของแอป (content root)
    /// </summary>
    public string RootPath { get; set; } = "uploads";
}
