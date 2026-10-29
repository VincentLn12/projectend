using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

/// <summary>Public, read-only news. Only published items are visible.</summary>
[ApiController]
[Route("api/news")]
public class NewsController(INewsService news) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<NewsListItemDto>> List([FromQuery] PageQuery query, CancellationToken ct) =>
        news.GetPublishedAsync(query, ct);

    [HttpGet("{id:guid}")]
    public Task<NewsDetailDto> Get(Guid id, CancellationToken ct) => news.GetPublishedByIdAsync(id, ct);
}
