using System.ComponentModel.DataAnnotations;
using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.News;

public class AdminNewsQuery : PageQuery
{
    public bool? IsPublished { get; set; }
    public DateOnly? PublishedFrom { get; set; }
    public DateOnly? PublishedTo { get; set; }

    [StringLength(30)]
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}
