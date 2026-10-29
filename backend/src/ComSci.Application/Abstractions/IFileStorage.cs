namespace ComSci.Application.Abstractions;

/// <summary>
/// ที่เก็บตัวไฟล์จริง ฐานข้อมูลเก็บแค่ path ที่ได้จากที่นี่
/// ตอนนี้เก็บบนดิสก์ (LocalFileStorage) ภายหลังเปลี่ยนเป็นที่เก็บอื่นได้โดยไม่ต้องแก้ service
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// บันทึกไฟล์ โดยระบบตั้งชื่อไฟล์เองจากนามสกุลที่ส่งมา
    /// </summary>
    /// <returns>path แบบ relative ที่ต้องเก็บไว้ในฐานข้อมูล</returns>
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken);

    /// <summary>
    /// เปิดไฟล์เพื่ออ่าน
    /// </summary>
    /// <returns>Stream ของไฟล์ หรือ null ถ้าไม่มีไฟล์นี้อยู่จริง</returns>
    Stream? OpenRead(string path);

    /// <summary>
    /// ลบไฟล์ (ถ้าไม่มีไฟล์อยู่แล้วก็ไม่ทำอะไร)
    /// </summary>
    Task DeleteAsync(string path, CancellationToken cancellationToken);
}
