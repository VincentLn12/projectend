using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.HomeBanners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/home-banners")]
public class AdminHomeBannersController : ControllerBase
{
    private readonly IHomeBannerService _bannerService;

    public AdminHomeBannersController(IHomeBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminHomeBannerDto>>> GetList(CancellationToken cancellationToken)
    {
        List<AdminHomeBannerDto> banners = await _bannerService.GetAllAsync(cancellationToken);
        return Ok(banners);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminHomeBannerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminHomeBannerDto banner = await _bannerService.GetByIdAsync(id, cancellationToken);
        return Ok(banner);
    }

    [HttpPost]
    public async Task<ActionResult<AdminHomeBannerDto>> Create(SaveHomeBannerRequest request, CancellationToken cancellationToken)
    {
        AdminHomeBannerDto created = await _bannerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminHomeBannerDto>> Update(
        Guid id,
        SaveHomeBannerRequest request,
        CancellationToken cancellationToken)
    {
        AdminHomeBannerDto updated = await _bannerService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminHomeBannerDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminHomeBannerDto updated = await _bannerService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _bannerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
