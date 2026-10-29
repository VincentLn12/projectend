using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/news")]
public class AdminNewsController(INewsService news) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<AdminNewsListItemDto>> List([FromQuery] AdminNewsQuery query, CancellationToken ct) =>
        news.GetAllAsync(query, ct);

    [HttpGet("{id:guid}")]
    public Task<AdminNewsDto> Get(Guid id, CancellationToken ct) => news.GetByIdAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<AdminNewsDto>> Create(SaveNewsRequest request, CancellationToken ct)
    {
        var created = await news.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public Task<AdminNewsDto> Update(Guid id, SaveNewsRequest request, CancellationToken ct) =>
        news.UpdateAsync(id, request, ct);

    [HttpPatch("{id:guid}/published")]
    public Task<AdminNewsDto> SetPublished(Guid id, SetPublishedRequest request, CancellationToken ct) =>
        news.SetPublishedAsync(id, request.IsPublished, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await news.DeleteAsync(id, ct);
        return NoContent();
    }
}
