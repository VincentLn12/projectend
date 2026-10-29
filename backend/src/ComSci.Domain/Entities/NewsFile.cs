namespace ComSci.Domain.Entities;

public class NewsFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NewsId { get; set; }
    public News News { get; set; } = null!;
    public Guid FileId { get; set; }
    public FileRecord File { get; set; } = null!;
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}
