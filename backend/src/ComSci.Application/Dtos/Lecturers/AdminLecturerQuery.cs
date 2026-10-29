using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.Lecturers;

public class AdminLecturerQuery : PageQuery
{
    public bool? IsPublished { get; set; }
}
