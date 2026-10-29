using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<NewsListItemDto>>> GetList(
        [FromQuery] PageQuery pageQuery,
        CancellationToken cancellationToken)
    {
        PagedResult<NewsListItemDto> result = await _newsService.GetPublishedAsync(pageQuery, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NewsDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        NewsDetailDto result = await _newsService.GetPublishedByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}
