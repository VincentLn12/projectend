namespace ComSci.Application.Dtos.HomeBanners;

public class AdminHomeBannerDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public Guid FileId { get; set; }
    public string? LinkUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
