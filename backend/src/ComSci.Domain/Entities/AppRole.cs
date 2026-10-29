using Microsoft.AspNetCore.Identity;

namespace ComSci.Domain.Entities;

/// <summary>
/// บทบาทผู้ใช้ (ตาราง Roles): User, Student, Lecturer, Admin
/// </summary>
public class AppRole : IdentityRole<Guid>
{
    /// <summary>constructor ว่างสำหรับ EF Core</summary>
    public AppRole()
    {
    }

    /// <summary>สร้างบทบาทใหม่จากชื่อ</summary>
    public AppRole(string name) : base(name)
    {
    }
}
