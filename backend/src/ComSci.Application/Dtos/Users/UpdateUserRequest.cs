using ComSci.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Users;

public class UpdateUserRequest
{
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    [StringLength(20)]
    public string? StudentCode { get; set; }
    [Range(1900, 2700)]
    public short? EntryYear { get; set; }
    [StringLength(200)]
    public string? PreviousSchool { get; set; }
    [StringLength(500)]
    public string? Address { get; set; }
}
