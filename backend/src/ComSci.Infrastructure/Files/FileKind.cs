namespace ComSci.Infrastructure.Files;

/// <summary>
/// ชนิดไฟล์ที่ระบบรองรับ พร้อมนามสกุลที่ใช้ได้กับชนิดนั้น
/// </summary>
public class FileKind
{
    /// <summary>ชนิดไฟล์แบบ MIME เช่น image/png</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// นามสกุลที่ใช้ได้ (ตัวพิมพ์เล็ก มีจุดนำหน้า) ตัวแรกคือนามสกุลที่ใช้ตอนเก็บไฟล์
    /// </summary>
    public List<string> Extensions { get; set; } = new List<string>();

    public FileKind(string contentType, params string[] extensions)
    {
        ContentType = contentType;
        Extensions = new List<string>(extensions);
    }
}
