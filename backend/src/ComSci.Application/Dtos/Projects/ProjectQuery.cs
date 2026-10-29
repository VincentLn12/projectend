using ComSci.Application.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Projects;

public class ProjectQuery : PageQuery
{
    public short? AcademicYear { get; set; }
    [StringLength(100)]
    public string? Keyword { get; set; }
}
