using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.JobHistories;

public class SaveJobHistoryRequest
{
    public Guid UserId { get; set; }
    [Required]
    [StringLength(200)]
    public string JobName { get; set; } = string.Empty;
    [StringLength(200)]
    public string? Company { get; set; }
    [StringLength(200)]
    public string? Position { get; set; }
    public DateOnly StartDate { get; set; }
    [StringLength(1000)]
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
}
