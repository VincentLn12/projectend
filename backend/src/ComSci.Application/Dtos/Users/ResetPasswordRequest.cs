using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Users;

public class ResetPasswordRequest
{
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}
