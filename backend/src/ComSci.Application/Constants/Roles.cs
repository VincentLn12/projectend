namespace ComSci.Application.Constants;

/// <summary>
/// ชื่อบทบาท (role) ทั้งหมดในระบบ ใช้ในโค้ดแทนการพิมพ์ข้อความตรงๆ
/// </summary>
public static class Roles
{
    /// <summary>ผู้ใช้ทั่วไป</summary>
    public const string User = "User";

    /// <summary>นักศึกษา</summary>
    public const string Student = "Student";

    /// <summary>อาจารย์</summary>
    public const string Lecturer = "Lecturer";

    /// <summary>ผู้ดูแลระบบ</summary>
    public const string Admin = "Admin";

    /// <summary>
    /// รายชื่อทุกบทบาท ใช้ตอน seed ข้อมูลเริ่มต้น
    /// </summary>
    public static readonly string[] All = new string[]
    {
        User,
        Student,
        Lecturer,
        Admin
    };
}
