using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.JobHistories;

public class AdminJobHistoryQuery : PageQuery
{
    public bool? IsPublished { get; set; }
}
