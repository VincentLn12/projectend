using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class HomeBanner : AuditableEntity
{
    public string? Title { get; set; }
    public Guid FileId { get; set; }
    public FileRecord File { get; set; } = null!;
    public string? LinkUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
}
