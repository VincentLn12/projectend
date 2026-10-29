using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class News : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string Body { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public AppUser Author { get; set; } = null!;
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<NewsFile> Files { get; set; } = new List<NewsFile>();
}
