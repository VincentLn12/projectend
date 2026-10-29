using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Syllabuses;

public class SubjectInput
{
    [StringLength(20)]
    public string? Code { get; set; }
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    [Range(0, 999)]
    public decimal Credit { get; set; }
    public byte SubjectCategory { get; set; }
}
