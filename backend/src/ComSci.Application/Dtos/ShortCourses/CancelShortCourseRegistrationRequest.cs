using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.ShortCourses;

public class CancelShortCourseRegistrationRequest
{
    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
}
