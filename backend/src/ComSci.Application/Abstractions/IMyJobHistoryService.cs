using ComSci.Application.Dtos.JobHistories;

namespace ComSci.Application.Abstractions;

public interface IMyJobHistoryService
{
    Task<List<AdminJobHistoryDto>> GetMineAsync(CancellationToken cancellationToken);

    Task<AdminJobHistoryDto> CreateMineAsync(SaveMyJobHistoryRequest request, CancellationToken cancellationToken);

    Task<AdminJobHistoryDto> UpdateMineAsync(Guid jobHistoryId, SaveMyJobHistoryRequest request, CancellationToken cancellationToken);

    Task DeleteMineAsync(Guid jobHistoryId, CancellationToken cancellationToken);
}
