using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.TechSubjects;

public class MoveTechSubjectRequest
{
    [Required]
    [RegularExpression("^(up|down)$", ErrorMessage = "Direction must be 'up' or 'down'.")]
    public string Direction { get; set; } = string.Empty;
}
