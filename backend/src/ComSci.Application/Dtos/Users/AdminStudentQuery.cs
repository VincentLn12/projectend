using System.ComponentModel.DataAnnotations;
using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.Users;

public class AdminStudentQuery : PageQuery
{
    public short? EntryYear { get; set; }

    public bool? IsLocked { get; set; }

    [StringLength(30)]
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; }
}
