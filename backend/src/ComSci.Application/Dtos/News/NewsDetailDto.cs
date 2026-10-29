namespace ComSci.Application.Dtos.News;

public class NewsDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public List<Guid> ImageFileIds { get; set; } = new List<Guid>();
}
