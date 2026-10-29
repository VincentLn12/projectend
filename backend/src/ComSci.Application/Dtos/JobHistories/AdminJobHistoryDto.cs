namespace ComSci.Application.Dtos.JobHistories;

public class AdminJobHistoryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Position { get; set; }
    public DateOnly StartDate { get; set; }
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
