using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.News;

public class AdminNewsQuery : PageQuery
{
    public bool? IsPublished { get; set; }
}
