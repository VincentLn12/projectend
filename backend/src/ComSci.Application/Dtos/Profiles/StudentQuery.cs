using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.Profiles;

public class StudentQuery : PageQuery
{
    public short? EntryYear { get; set; }
}
