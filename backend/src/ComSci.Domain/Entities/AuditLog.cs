namespace ComSci.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Changes { get; set; }
    public DateTime CreatedAt { get; set; }
}
