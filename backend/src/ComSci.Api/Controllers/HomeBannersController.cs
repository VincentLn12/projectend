using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.HomeBanners;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/home-banners")]
public class HomeBannersController : ControllerBase
{
    private readonly IHomeBannerService _bannerService;

    public HomeBannersController(IHomeBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<HomeBannerDto>>> GetList(CancellationToken cancellationToken)
    {
        List<HomeBannerDto> banners = await _bannerService.GetPublishedAsync(cancellationToken);
        return Ok(banners);
    }
}
