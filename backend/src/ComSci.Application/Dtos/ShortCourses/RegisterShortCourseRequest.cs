using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.ShortCourses;

public class RegisterShortCourseRequest
{
    [StringLength(50)]
    public string? Prefix { get; set; }
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
}
