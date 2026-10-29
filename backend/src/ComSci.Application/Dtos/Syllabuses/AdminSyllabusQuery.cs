using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.Syllabuses;

public class AdminSyllabusQuery : PageQuery
{
    public bool? IsPublished { get; set; }
}
