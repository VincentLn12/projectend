using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.ShortCourses;

public class ShortCourseCertificateRequest
{
    public Guid RegistrationId { get; set; }
    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
}
