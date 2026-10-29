using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.JobHistories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/job-histories")]
public class AdminJobHistoriesController : ControllerBase
{
    private readonly IJobHistoryService _jobHistoryService;

    public AdminJobHistoriesController(IJobHistoryService jobHistoryService)
    {
        _jobHistoryService = jobHistoryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminJobHistoryDto>>> GetList(
        [FromQuery] AdminJobHistoryQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminJobHistoryDto> result = await _jobHistoryService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminJobHistoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminJobHistoryDto result = await _jobHistoryService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminJobHistoryDto>> Create(SaveJobHistoryRequest request, CancellationToken cancellationToken)
    {
        AdminJobHistoryDto created = await _jobHistoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminJobHistoryDto>> Update(
        Guid id,
        SaveJobHistoryRequest request,
        CancellationToken cancellationToken)
    {
        AdminJobHistoryDto updated = await _jobHistoryService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminJobHistoryDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminJobHistoryDto updated = await _jobHistoryService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _jobHistoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
