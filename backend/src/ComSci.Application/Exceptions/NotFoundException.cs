namespace ComSci.Application.Exceptions;

/// <summary>
/// ไม่พบข้อมูลที่ขอ (หรือถูก soft delete แล้ว) ระบบแปลงเป็น HTTP 404
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
