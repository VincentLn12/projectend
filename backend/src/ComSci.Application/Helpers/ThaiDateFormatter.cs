namespace ComSci.Application.Helpers;

public static class ThaiDateFormatter
{
    private const int ThailandUtcOffsetHours = 7;
    private const int BuddhistEraOffset = 543;

    private static readonly string[] MonthNames = new string[]
    {
        "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน",
        "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม"
    };

    public static string FormatDate(DateTime utcValue)
    {
        DateTime local = ToThailandTime(utcValue);
        return local.Day + " " + MonthNames[local.Month - 1] + " " + (local.Year + BuddhistEraOffset);
    }

    public static string FormatRange(DateTime utcStart, DateTime utcEnd)
    {
        DateTime start = ToThailandTime(utcStart);
        DateTime end = ToThailandTime(utcEnd);

        if (start.Date == end.Date)
        {
            return FormatDate(utcStart);
        }

        if (start.Year == end.Year && start.Month == end.Month)
        {
            return start.Day + "-" + end.Day + " " + MonthNames[start.Month - 1] + " " + (start.Year + BuddhistEraOffset);
        }

        return FormatDate(utcStart) + " - " + FormatDate(utcEnd);
    }

    private static DateTime ToThailandTime(DateTime utcValue)
    {
        return DateTime.SpecifyKind(utcValue, DateTimeKind.Utc).AddHours(ThailandUtcOffsetHours);
    }
}
