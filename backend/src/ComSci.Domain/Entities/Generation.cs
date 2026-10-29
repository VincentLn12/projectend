using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class Generation : AuditableEntity
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Quantity { get; set; }
    public decimal? Fee { get; set; }
    public Guid? PhotoFileId { get; set; }
    public FileRecord? PhotoFile { get; set; }
    public bool IsCancelled { get; set; }
    public ICollection<CourseRegistration> Registrations { get; set; } = new List<CourseRegistration>();
    public ICollection<CourseComment> Comments { get; set; } = new List<CourseComment>();
}
