namespace ComSci.Application.Dtos.Courses;

public class GenerationDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Quantity { get; set; }
    public int RegisteredCount { get; set; }
    public decimal? Fee { get; set; }
    public Guid? PhotoFileId { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsRegistrationOpen { get; set; }
}
