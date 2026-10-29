using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.News;

public class SaveNewsRequest
{
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;
    [StringLength(500)]
    public string? SubTitle { get; set; }
    [Required]
    public string Body { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public List<Guid>? FileIds { get; set; }
    public Guid? MainFileId { get; set; }
}
