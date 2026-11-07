using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.ShortCourses;

public class SaveShortCourseRequest
{
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [StringLength(500)]
    public string? Audience { get; set; }
    [StringLength(500)]
    public string? Location { get; set; }
    [StringLength(300)]
    public string? TeacherName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [Range(1, 1000)]
    public int? Hours { get; set; }
    [Range(0, 100000)]
    public int MaxParticipants { get; set; }
    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }
    public bool IsRegistrationEnabled { get; set; }
    public bool IsPublished { get; set; }
    public Guid? CoverFileId { get; set; }
    public Guid? CertificateTemplateId { get; set; }
}
