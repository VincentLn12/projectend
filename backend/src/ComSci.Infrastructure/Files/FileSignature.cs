namespace ComSci.Infrastructure.Files;

/// <summary>
/// ตรวจชนิดไฟล์จากไบต์ต้นไฟล์ (magic bytes)
/// เหตุผล: ชื่อไฟล์และ Content-Type ที่ client ส่งมาปลอมได้ แต่เนื้อไฟล์ปลอมยากกว่า
/// </summary>
public static class FileSignature
{
    private static readonly FileKind Png = new FileKind("image/png", ".png");
    private static readonly FileKind Jpeg = new FileKind("image/jpeg", ".jpg", ".jpeg");
    private static readonly FileKind Gif = new FileKind("image/gif", ".gif");
    private static readonly FileKind Webp = new FileKind("image/webp", ".webp");
    private static readonly FileKind Pdf = new FileKind("application/pdf", ".pdf");

    private static readonly byte[] PngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] JpegSignature = new byte[] { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] Gif87Signature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }; // "GIF87a"
    private static readonly byte[] Gif89Signature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // "GIF89a"
    private static readonly byte[] RiffSignature = new byte[] { 0x52, 0x49, 0x46, 0x46 };              // "RIFF"
    private static readonly byte[] WebpSignature = new byte[] { 0x57, 0x45, 0x42, 0x50 };              // "WEBP"
    private static readonly byte[] PdfSignature = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D };         // "%PDF-"

    /// <summary>
    /// ดูไบต์ต้นไฟล์แล้วบอกว่าเป็นไฟล์ชนิดไหน
    /// </summary>
    /// <param name="header">ไบต์ต้นไฟล์ (อย่างน้อย 12 ไบต์ถ้ามี)</param>
    /// <param name="headerLength">จำนวนไบต์ที่อ่านได้จริงใน header</param>
    /// <returns>ชนิดไฟล์ หรือ null ถ้าไม่ใช่ชนิดที่รองรับ</returns>
    public static FileKind? Detect(byte[] header, int headerLength)
    {
        if (StartsWith(header, headerLength, 0, PngSignature))
        {
            return Png;
        }

        if (StartsWith(header, headerLength, 0, JpegSignature))
        {
            return Jpeg;
        }

        if (StartsWith(header, headerLength, 0, Gif87Signature) || StartsWith(header, headerLength, 0, Gif89Signature))
        {
            return Gif;
        }

        // WebP คือไฟล์ RIFF ที่ไบต์ที่ 8-11 เป็นคำว่า WEBP
        if (StartsWith(header, headerLength, 0, RiffSignature) && StartsWith(header, headerLength, 8, WebpSignature))
        {
            return Webp;
        }

        if (StartsWith(header, headerLength, 0, PdfSignature))
        {
            return Pdf;
        }

        return null;
    }

    /// <summary>
    /// ตรวจว่า header ตั้งแต่ตำแหน่ง offset ตรงกับ signature หรือไม่
    /// </summary>
    private static bool StartsWith(byte[] header, int headerLength, int offset, byte[] signature)
    {
        if (headerLength < offset + signature.Length)
        {
            return false;
        }

        for (int index = 0; index < signature.Length; index++)
        {
            if (header[offset + index] != signature[index])
            {
                return false;
            }
        }

        return true;
    }
}
