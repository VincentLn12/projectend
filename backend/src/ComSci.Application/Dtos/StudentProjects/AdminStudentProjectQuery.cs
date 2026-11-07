using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.StudentProjects;

public class AdminStudentProjectQuery : PageQuery
{
    public bool? IsPublished { get; set; }
    public Guid? StudentId { get; set; }
    public short? AcademicYear { get; set; }
    public byte? Semester { get; set; }
}
