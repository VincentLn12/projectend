using ComSci.Domain.Entities;

namespace ComSci.Application.Helpers;

public static class CertificateVariables
{
    public const string FullName = "{ชื่อ-นามสกุล}";
    public const string CourseTitle = "{ชื่อหลักสูตร}";
    public const string CourseDates = "{วันที่อบรม}";
    public const string Hours = "{จำนวนชั่วโมง}";
    public const string Location = "{สถานที่}";
    public const string Teacher = "{วิทยากร}";

    public static IReadOnlyDictionary<string, string> ForCourse(ShortCourse shortCourse, string fullName)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values[FullName] = fullName;
        values[CourseTitle] = shortCourse.Title;
        values[CourseDates] = ThaiDateFormatter.FormatRange(shortCourse.StartDate, shortCourse.EndDate);
        values[Hours] = shortCourse.Hours.HasValue ? "จำนวน " + shortCourse.Hours.Value + " ชั่วโมง" : string.Empty;
        values[Location] = shortCourse.Location ?? string.Empty;
        values[Teacher] = shortCourse.TeacherName ?? string.Empty;
        return values;
    }

    public static IReadOnlyDictionary<string, string> Sample()
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values[FullName] = "นายสมชาย ใจดี";
        values[CourseTitle] = "การเขียนโปรแกรมเบื้องต้น";
        values[CourseDates] = "1 – 2 มกราคม 2570";
        values[Hours] = "จำนวน 12 ชั่วโมง";
        values[Location] = "ห้องปฏิบัติการคอมพิวเตอร์ อาคาร 9";
        values[Teacher] = "อาจารย์ตัวอย่าง";
        return values;
    }

    public static string Apply(string text, IReadOnlyDictionary<string, string> values)
    {
        string result = text;
        foreach (KeyValuePair<string, string> pair in values)
        {
            result = result.Replace(pair.Key, pair.Value);
        }

        return result;
    }
}
