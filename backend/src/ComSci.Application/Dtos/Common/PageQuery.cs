using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Common;

public class PageQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
    [StringLength(200)]
    public string? Search { get; set; }
}
