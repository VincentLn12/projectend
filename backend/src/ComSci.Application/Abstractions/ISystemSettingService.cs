using ComSci.Application.Dtos.Settings;

namespace ComSci.Application.Abstractions;

public interface ISystemSettingService
{
    Task<SystemSettingDto> GetAsync(CancellationToken cancellationToken);

    Task<SystemSettingDto> UpdateAsync(SaveSystemSettingRequest request, CancellationToken cancellationToken);
}
