using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.StudentProjects;

public class StudentProjectQuery : PageQuery
{
    public short? AcademicYear { get; set; }
    public byte? Semester { get; set; }
}
