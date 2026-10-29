namespace ComSci.Application.Constants;

/// <summary>
/// กฎของการอัปโหลดไฟล์
/// </summary>
public static class FileRules
{
    /// <summary>ขนาดไฟล์สูงสุดที่รับได้ (10 MB)</summary>
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    /// <summary>
    /// ขนาด request สูงสุด = ขนาดไฟล์สูงสุด + พื้นที่เผื่อส่วนหัวของ multipart (512 KB)
    /// </summary>
    public const long MaxRequestBytes = MaxUploadBytes + 512 * 1024;
}
