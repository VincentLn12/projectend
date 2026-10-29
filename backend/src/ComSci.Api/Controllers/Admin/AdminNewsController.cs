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
public class AdminNewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public AdminNewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminNewsListItemDto>>> GetList(
        [FromQuery] AdminNewsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminNewsListItemDto> result = await _newsService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminNewsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminNewsDto result = await _newsService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminNewsDto>> Create(SaveNewsRequest request, CancellationToken cancellationToken)
    {
        AdminNewsDto created = await _newsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminNewsDto>> Update(
        Guid id,
        SaveNewsRequest request,
        CancellationToken cancellationToken)
    {
        AdminNewsDto updated = await _newsService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminNewsDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminNewsDto updated = await _newsService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _newsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
