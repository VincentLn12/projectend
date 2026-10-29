namespace ComSci.Application.Exceptions;

/// <summary>
/// คำขอรูปแบบถูกต้องแต่ผิดกฎทางธุรกิจ ระบบแปลงเป็น HTTP 400
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}
