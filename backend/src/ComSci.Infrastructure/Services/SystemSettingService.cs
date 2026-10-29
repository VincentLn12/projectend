using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Settings;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly AppDbContext _dbContext;

    public SystemSettingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SystemSettingDto> GetAsync(CancellationToken cancellationToken)
    {
        SystemSetting? setting = await _dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (setting == null)
        {
            throw new NotFoundException("Settings not found.");
        }

        return ToDto(setting);
    }

    public async Task<SystemSettingDto> UpdateAsync(SaveSystemSettingRequest request, CancellationToken cancellationToken)
    {
        // ตารางนี้มีแถวเดียว สร้างไว้ตอนเริ่มระบบโดย DbSeeder
        SystemSetting? setting = await _dbContext.SystemSettings.FirstOrDefaultAsync(cancellationToken);
        if (setting == null)
        {
            throw new NotFoundException("Settings not found.");
        }

        if (request.LogoFileId.HasValue)
        {
            bool logoExists = await _dbContext.Files.AnyAsync(file => file.Id == request.LogoFileId.Value, cancellationToken);
            if (!logoExists)
            {
                throw new BadRequestException("Logo file does not exist.");
            }
        }

        setting.WebName = request.WebName.Trim();
        setting.RegisterUrl = request.RegisterUrl?.Trim();
        setting.KruUrl = request.KruUrl?.Trim();
        setting.PageFacebook = request.PageFacebook?.Trim();
        setting.LogoFileId = request.LogoFileId;
        setting.VideoUrl = request.VideoUrl?.Trim();
        setting.MapLocation = request.MapLocation?.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(setting);
    }

    private static SystemSettingDto ToDto(SystemSetting setting)
    {
        SystemSettingDto dto = new SystemSettingDto();
        dto.WebName = setting.WebName;
        dto.RegisterUrl = setting.RegisterUrl;
        dto.KruUrl = setting.KruUrl;
        dto.PageFacebook = setting.PageFacebook;
        dto.LogoFileId = setting.LogoFileId;
        dto.VideoUrl = setting.VideoUrl;
        dto.MapLocation = setting.MapLocation;
        return dto;
    }
}
