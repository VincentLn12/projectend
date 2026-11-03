using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Settings;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using System.Text.Json;
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

        if (request.QuickMenuBackgroundFileId.HasValue)
        {
            bool backgroundExists = await _dbContext.Files.AnyAsync(file => file.Id == request.QuickMenuBackgroundFileId.Value, cancellationToken);
            if (!backgroundExists)
            {
                throw new BadRequestException("Quick menu background file does not exist.");
            }
        }

        setting.WebName = request.WebName.Trim();
        setting.RegisterUrl = request.RegisterUrl?.Trim();
        setting.KruUrl = request.KruUrl?.Trim();
        setting.PageFacebook = request.PageFacebook?.Trim();
        setting.LogoFileId = request.LogoFileId;
        setting.VideoUrl = request.VideoUrl?.Trim();
        setting.MapLocation = request.MapLocation?.Trim();
        setting.AboutTitle = string.IsNullOrWhiteSpace(request.AboutTitle) ? null : request.AboutTitle.Trim();
        setting.AboutDescription = string.IsNullOrWhiteSpace(request.AboutDescription) ? null : request.AboutDescription.Trim();
        setting.QuickMenuEnabled = request.QuickMenuEnabled;
        setting.QuickMenuBackgroundFileId = request.QuickMenuBackgroundFileId;
        setting.QuickMenuItems = SerializeQuickMenuItems(request.QuickMenuItems);
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
        dto.AboutTitle = setting.AboutTitle;
        dto.AboutDescription = setting.AboutDescription;
        dto.QuickMenuEnabled = setting.QuickMenuEnabled;
        dto.QuickMenuBackgroundFileId = setting.QuickMenuBackgroundFileId;
        dto.QuickMenuItems = DeserializeQuickMenuItems(setting.QuickMenuItems);
        return dto;
    }

    private static string? SerializeQuickMenuItems(List<QuickMenuItemDto>? items)
    {
        if (items == null || items.Count == 0)
        {
            return null;
        }

        List<QuickMenuItemDto> distinctItems = items
            .GroupBy(item => item.Key.Trim())
            .Select(group => new QuickMenuItemDto { Key = group.Key, Enabled = group.First().Enabled })
            .ToList();
        return JsonSerializer.Serialize(distinctItems);
    }

    private static List<QuickMenuItemDto> DeserializeQuickMenuItems(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<QuickMenuItemDto>();
        }

        return JsonSerializer.Deserialize<List<QuickMenuItemDto>>(json) ?? new List<QuickMenuItemDto>();
    }
}
