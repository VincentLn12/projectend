using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Courses;

public class SaveCourseRequest
{
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LecturerId { get; set; }
    public bool IsPublished { get; set; }
    public List<Guid>? FileIds { get; set; }
    public Guid? MainFileId { get; set; }
}
