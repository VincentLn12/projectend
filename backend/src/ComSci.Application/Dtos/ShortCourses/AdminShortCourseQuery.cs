using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.ShortCourses;

public class AdminShortCourseQuery : PageQuery
{
    public bool? IsPublished { get; set; }
}
