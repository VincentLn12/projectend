using ComSci.Application.Dtos.Students;

namespace ComSci.Application.Abstractions;

public interface IStudentApprovalService
{
    Task<List<PendingStudentDto>> GetPendingAsync(CancellationToken cancellationToken);

    Task ApproveAsync(Guid userId, CancellationToken cancellationToken);

    Task RejectAsync(Guid userId, CancellationToken cancellationToken);
}
