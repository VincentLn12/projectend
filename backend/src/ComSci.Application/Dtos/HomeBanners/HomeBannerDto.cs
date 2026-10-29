namespace ComSci.Application.Dtos.HomeBanners;

public class HomeBannerDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public Guid FileId { get; set; }
    public string? LinkUrl { get; set; }
}
