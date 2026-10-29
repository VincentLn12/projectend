using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.Courses;

public class AdminCourseQuery : PageQuery
{
    public bool? IsPublished { get; set; }
}
