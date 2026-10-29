namespace ComSci.Application.Options;

/// <summary>
/// ค่าตั้งค่า JWT อ่านจาก section "Jwt" ใน appsettings (Key เก็บใน user-secrets)
/// </summary>
public class JwtOptions
{
    /// <summary>ชื่อ section ใน appsettings</summary>
    public const string SectionName = "Jwt";

    /// <summary>ผู้ออก token</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>ผู้รับ token</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>กุญแจเซ็นลายเซ็น (ยาวอย่างน้อย 32 ตัวอักษร)</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>อายุ access token เป็นนาที</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>อายุ refresh token เป็นวัน</summary>
    public int RefreshTokenDays { get; set; } = 7;
}
