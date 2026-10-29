namespace ComSci.Application.Constants;

public static class Roles
{
    public const string User = "User";

    public const string Student = "Student";

    public const string Lecturer = "Lecturer";

    public const string Admin = "Admin";

    public static readonly string[] All = new string[]
    {
        User,
        Student,
        Lecturer,
        Admin
    };
}
