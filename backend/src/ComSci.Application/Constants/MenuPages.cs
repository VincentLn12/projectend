namespace ComSci.Application.Constants;

public class MenuPageOption
{
    public MenuPageOption(string path, string label)
    {
        Path = path;
        Label = label;
    }

    public string Path { get; }
    public string Label { get; }
}

public static class MenuPages
{
    // หน้าในระบบที่เมนูลิงก์ไปได้ และเป็นเมนูเริ่มต้นของเว็บใหม่
    public static readonly IReadOnlyList<MenuPageOption> All = new List<MenuPageOption>
    {
        new MenuPageOption("/", "หน้าแรก"),
        new MenuPageOption("/news", "ข่าว"),
        new MenuPageOption("/projects", "โครงงาน"),
        new MenuPageOption("/syllabus", "หลักสูตร"),
        new MenuPageOption("/courses", "อบรมระยะสั้น"),
        new MenuPageOption("/tech-subjects", "เทคโนโลยี"),
        new MenuPageOption("/lecturers", "อาจารย์")
    };

    public const int MaxMenuItems = 40;
}
