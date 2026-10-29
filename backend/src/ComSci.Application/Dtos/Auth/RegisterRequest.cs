using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
    [Range(typeof(bool), "true", "true", ErrorMessage = "Privacy policy must be accepted.")]
    public bool AcceptPrivacy { get; set; }
}
