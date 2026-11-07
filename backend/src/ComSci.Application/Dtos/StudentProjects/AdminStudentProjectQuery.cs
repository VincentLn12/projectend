using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.StudentProjects;

public class AdminStudentProjectQuery : PageQuery
{
    public bool? IsPublished { get; set; }
}
