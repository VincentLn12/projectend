using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.ShortCourses;

public class LookupShortCourseRegistrationRequest
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
}
