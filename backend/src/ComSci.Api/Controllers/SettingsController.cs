using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Settings;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISystemSettingService _settingService;

    public SettingsController(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<ActionResult<SystemSettingDto>> Get(CancellationToken cancellationToken)
    {
        SystemSettingDto setting = await _settingService.GetAsync(cancellationToken);
        return Ok(setting);
    }
}
