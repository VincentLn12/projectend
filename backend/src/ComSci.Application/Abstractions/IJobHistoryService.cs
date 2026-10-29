using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.JobHistories;

namespace ComSci.Application.Abstractions;

public interface IJobHistoryService
{
    Task<PagedResult<JobHistoryListItemDto>> GetPublishedAsync(PageQuery query, CancellationToken cancellationToken);

    Task<List<JobSummaryItemDto>> GetSummaryAsync(CancellationToken cancellationToken);

    Task<PagedResult<AdminJobHistoryDto>> GetAllAsync(AdminJobHistoryQuery query, CancellationToken cancellationToken);

    Task<AdminJobHistoryDto> GetByIdAsync(Guid jobHistoryId, CancellationToken cancellationToken);

    Task<AdminJobHistoryDto> CreateAsync(SaveJobHistoryRequest request, CancellationToken cancellationToken);

    Task<AdminJobHistoryDto> UpdateAsync(Guid jobHistoryId, SaveJobHistoryRequest request, CancellationToken cancellationToken);

    Task<AdminJobHistoryDto> SetPublishedAsync(Guid jobHistoryId, bool isPublished, CancellationToken cancellationToken);

    Task DeleteAsync(Guid jobHistoryId, CancellationToken cancellationToken);
}
