namespace ComSci.Domain.Entities;

/// <summary>
/// ตั้งค่าเว็บ มีแถวเดียวทั้งระบบ
/// </summary>
public class SystemSetting
{
    /// <summary>รหัสหลักของแถว</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ชื่อเว็บ</summary>
    public string WebName { get; set; } = string.Empty;

    /// <summary>ลิงก์ระบบรับสมัครเรียน</summary>
    public string? RegisterUrl { get; set; }

    /// <summary>ลิงก์เว็บมหาวิทยาลัย</summary>
    public string? KruUrl { get; set; }

    /// <summary>ลิงก์เพจ Facebook</summary>
    public string? PageFacebook { get; set; }

    /// <summary>โลโก้ (FK, ไม่บังคับ)</summary>
    public Guid? LogoFileId { get; set; }

    /// <summary>ข้อมูลที่อ้างถึง: โลโก้</summary>
    public FileRecord? LogoFile { get; set; }

    /// <summary>ลิงก์วิดีโอ YouTube</summary>
    public string? VideoUrl { get; set; }

    /// <summary>พิกัดแผนที่ (ไม่เก็บ access token ของ Mapbox)</summary>
    public string? MapLocation { get; set; }
}
