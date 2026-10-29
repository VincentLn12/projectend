namespace ComSci.Application.Dtos.News;

public class AdminNewsListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
