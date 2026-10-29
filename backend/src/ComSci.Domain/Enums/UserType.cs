namespace ComSci.Domain.Enums;

/// <summary>
/// ประเภทผู้ใช้ (เก็บเป็นตัวเลขในฐานข้อมูล)
/// </summary>
public enum UserType : byte
{
    /// <summary>ผู้ใช้ทั่วไป</summary>
    General = 1,

    /// <summary>นักศึกษา</summary>
    Student = 2,

    /// <summary>อาจารย์</summary>
    Lecturer = 3,

    /// <summary>ผู้ดูแลระบบ</summary>
    Admin = 4
}
