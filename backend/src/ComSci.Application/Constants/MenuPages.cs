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
    // หน้าสาธารณะที่เมนูลิงก์ไปได้ ต้องตรงกับเส้นทางใน frontend (app.routes.ts)
    public static readonly IReadOnlyList<MenuPageOption> All = new List<MenuPageOption>
    {
        new MenuPageOption("/home", "หน้าหลัก"),
        new MenuPageOption("/syllabus", "หลักสูตรวิทย์คอม"),
        new MenuPageOption("/projects", "โครงการงานวิจัย"),
        new MenuPageOption("/student-projects", "โครงงานนักศึกษา"),
        new MenuPageOption("/short-courses", "หลักสูตรอบรม"),
        new MenuPageOption("/lecturers", "อาจารย์"),
        new MenuPageOption("/news", "ข่าวสาร")
    };

    // เมนูเริ่มต้นของเว็บใหม่: ทุกหน้าข้างบนยกเว้นข่าวสาร (ข่าวแสดงในหน้าหลักอยู่แล้ว)
    public static readonly IReadOnlyList<MenuPageOption> Defaults = All.Where(page => page.Path != "/news").ToList();

    public const int MaxMenuItems = 40;
}
