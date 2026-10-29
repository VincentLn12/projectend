using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Auth;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
