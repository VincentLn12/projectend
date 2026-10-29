using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.JobHistories;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/job-histories")]
public class JobHistoriesController : ControllerBase
{
    private readonly IJobHistoryService _jobHistoryService;

    public JobHistoriesController(IJobHistoryService jobHistoryService)
    {
        _jobHistoryService = jobHistoryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<JobHistoryListItemDto>>> GetList(
        [FromQuery] PageQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<JobHistoryListItemDto> result = await _jobHistoryService.GetPublishedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<List<JobSummaryItemDto>>> GetSummary(CancellationToken cancellationToken)
    {
        List<JobSummaryItemDto> summary = await _jobHistoryService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}
