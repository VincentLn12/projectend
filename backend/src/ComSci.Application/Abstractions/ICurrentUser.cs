namespace ComSci.Application.Abstractions;

/// <summary>
/// บอกว่าใครกำลังเรียกใช้งานอยู่ (อ่านจาก JWT ของ request ปัจจุบัน)
/// ใช้ใน service และ interceptor โดยไม่ต้องรู้จัก HttpContext
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// รหัสผู้ใช้ที่ล็อกอินอยู่ หรือ null ถ้าไม่ได้ล็อกอิน (เช่น ตอน seed ข้อมูล)
    /// </summary>
    Guid? UserId { get; }
}
