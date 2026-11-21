using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.HomeBanners;

public class MoveHomeBannerRequest
{
    [Required]
    [RegularExpression("^(up|down)$", ErrorMessage = "Direction must be 'up' or 'down'.")]
    public string Direction { get; set; } = string.Empty;
}
