namespace ComSci.Application.Dtos.News;

public class AdminNewsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<NewsFileDto> Files { get; set; } = new List<NewsFileDto>();
}
