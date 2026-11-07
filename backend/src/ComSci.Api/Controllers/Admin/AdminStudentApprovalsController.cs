using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/student-approvals")]
public class AdminStudentApprovalsController : ControllerBase
{
    private readonly IStudentApprovalService _approvalService;

    public AdminStudentApprovalsController(IStudentApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PendingStudentDto>>> GetPending(CancellationToken cancellationToken)
    {
        List<PendingStudentDto> result = await _approvalService.GetPendingAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("{userId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid userId, CancellationToken cancellationToken)
    {
        await _approvalService.ApproveAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Reject(Guid userId, CancellationToken cancellationToken)
    {
        await _approvalService.RejectAsync(userId, cancellationToken);
        return NoContent();
    }
}
