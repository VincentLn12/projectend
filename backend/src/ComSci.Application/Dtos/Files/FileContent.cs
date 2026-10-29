using System.IO;

namespace ComSci.Application.Dtos.Files;

/// <summary>
/// ไฟล์ที่เปิดไว้สำหรับอ่าน ผู้เรียกต้องเป็นคนปิด Stream
/// </summary>
public class FileContent
{
    /// <summary>ข้อมูลไฟล์</summary>
    public Stream Content { get; set; } = Stream.Null;

    /// <summary>ชนิดไฟล์ (MIME)</summary>
    public string ContentType { get; set; } = string.Empty;
}
