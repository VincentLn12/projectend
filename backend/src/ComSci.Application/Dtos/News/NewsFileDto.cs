namespace ComSci.Application.Dtos.News;

public class NewsFileDto
{
    public Guid FileId { get; set; }
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}
