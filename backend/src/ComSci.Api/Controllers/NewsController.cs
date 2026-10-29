using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

/// <summary>
/// ข่าวประชาสัมพันธ์สำหรับคนทั่วไป (อ่านอย่างเดียว) เห็นเฉพาะข่าวที่เผยแพร่แล้ว
/// </summary>
[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    /// <summary>
    /// รายการข่าว เรียงจากใหม่ไปเก่า ค้นหาจากหัวข้อด้วย ?search=
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<NewsListItemDto>>> GetList(
        [FromQuery] PageQuery pageQuery,
        CancellationToken cancellationToken)
    {
        PagedResult<NewsListItemDto> result = await _newsService.GetPublishedAsync(pageQuery, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// รายละเอียดข่าวหนึ่งรายการ
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NewsDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        NewsDetailDto result = await _newsService.GetPublishedByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}
