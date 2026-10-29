using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Courses;

public class SaveGenerationRequest
{
    [StringLength(300)]
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [Range(1, 100000)]
    public int Quantity { get; set; }
    [Range(0, 10000000)]
    public decimal? Fee { get; set; }
    public Guid? PhotoFileId { get; set; }
    public bool IsCancelled { get; set; }
}
