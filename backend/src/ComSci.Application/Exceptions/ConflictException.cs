namespace ComSci.Application.Exceptions;

/// <summary>
/// การกระทำขัดกับสถานะข้อมูลปัจจุบัน เช่น ลบข้อมูลที่ยังถูกอ้างอิงอยู่ ระบบแปลงเป็น HTTP 409
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
