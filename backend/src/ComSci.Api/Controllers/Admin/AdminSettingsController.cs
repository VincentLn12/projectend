using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/settings")]
public class AdminSettingsController : ControllerBase
{
    private readonly ISystemSettingService _settingService;

    public AdminSettingsController(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpPut]
    public async Task<ActionResult<SystemSettingDto>> Update(SaveSystemSettingRequest request, CancellationToken cancellationToken)
    {
        SystemSettingDto updated = await _settingService.UpdateAsync(request, cancellationToken);
        return Ok(updated);
    }
}
