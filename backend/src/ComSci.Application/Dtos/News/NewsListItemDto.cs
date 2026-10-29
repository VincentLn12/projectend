namespace ComSci.Application.Dtos.News;

public class NewsListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid? MainImageFileId { get; set; }
}
