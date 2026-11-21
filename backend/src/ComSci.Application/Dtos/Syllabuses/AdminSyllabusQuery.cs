using System.ComponentModel.DataAnnotations;
using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.Syllabuses;

public class AdminSyllabusQuery : PageQuery
{
    public bool? IsPublished { get; set; }

    [StringLength(30)]
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = true;
}
