using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.HomeBanners;

public class SaveHomeBannerRequest
{
    [StringLength(200)]
    public string? Title { get; set; }
    public Guid FileId { get; set; }
    [Url]
    [StringLength(500)]
    public string? LinkUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
}
