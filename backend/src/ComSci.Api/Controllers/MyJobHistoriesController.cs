using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.JobHistories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Student)]
[Route("api/me/job-histories")]
public class MyJobHistoriesController : ControllerBase
{
    private readonly IMyJobHistoryService _myJobHistoryService;

    public MyJobHistoriesController(IMyJobHistoryService myJobHistoryService)
    {
        _myJobHistoryService = myJobHistoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminJobHistoryDto>>> GetMine(CancellationToken cancellationToken)
    {
        List<AdminJobHistoryDto> result = await _myJobHistoryService.GetMineAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminJobHistoryDto>> Create(SaveMyJobHistoryRequest request, CancellationToken cancellationToken)
    {
        AdminJobHistoryDto created = await _myJobHistoryService.CreateMineAsync(request, cancellationToken);
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminJobHistoryDto>> Update(Guid id, SaveMyJobHistoryRequest request, CancellationToken cancellationToken)
    {
        AdminJobHistoryDto updated = await _myJobHistoryService.UpdateMineAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _myJobHistoryService.DeleteMineAsync(id, cancellationToken);
        return NoContent();
    }
}
