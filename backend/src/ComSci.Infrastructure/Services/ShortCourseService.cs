using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.ShortCourses;
using ComSci.Application.Exceptions;
using ComSci.Application.Helpers;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class ShortCourseService : IShortCourseService
{
    private readonly AppDbContext _dbContext;

    public ShortCourseService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ShortCourseListItemDto>> GetPublishedAsync(PageQuery query, CancellationToken cancellationToken)
    {
        IQueryable<ShortCourse> shortCourses = _dbContext.ShortCourses
            .AsNoTracking()
            .Where(shortCourse => shortCourse.IsPublished);

        shortCourses = ApplySearch(shortCourses, query.Search);

        int total = await shortCourses.CountAsync(cancellationToken);

        var rows = await shortCourses
            .OrderByDescending(shortCourse => shortCourse.StartDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(shortCourse => new
            {
                shortCourse.Id,
                shortCourse.Title,
                shortCourse.CoverFileId,
                shortCourse.StartDate,
                shortCourse.EndDate,
                shortCourse.Hours,
                shortCourse.Location,
                shortCourse.MaxParticipants,
                shortCourse.IsRegistrationEnabled,
                shortCourse.RegistrationOpenAt,
                shortCourse.RegistrationCloseAt,
                RegisteredCount = shortCourse.Registrations.Count(registration => registration.Status != RegistrationStatus.Cancelled)
            })
            .ToListAsync(cancellationToken);

        DateTime now = DateTime.UtcNow;
        PagedResult<ShortCourseListItemDto> result = new PagedResult<ShortCourseListItemDto>();
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;

        foreach (var row in rows)
        {
            ShortCourseListItemDto item = new ShortCourseListItemDto();
            item.Id = row.Id;
            item.Title = row.Title;
            item.CoverFileId = row.CoverFileId;
            item.StartDate = row.StartDate;
            item.EndDate = row.EndDate;
            item.Hours = row.Hours;
            item.Location = row.Location;
            item.MaxParticipants = row.MaxParticipants;
            item.RegisteredCount = row.RegisteredCount;
            item.Availability = ShortCourseAvailabilityCalculator.Calculate(
                row.IsRegistrationEnabled,
                row.RegistrationOpenAt,
                row.RegistrationCloseAt,
                row.StartDate,
                row.MaxParticipants,
                row.RegisteredCount,
                now);
            result.Items.Add(item);
        }

        return result;
    }

    public async Task<ShortCourseDetailDto> GetPublishedByIdAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        ShortCourse? shortCourse = await _dbContext.ShortCourses
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == shortCourseId && item.IsPublished, cancellationToken);
        if (shortCourse == null)
        {
            throw new NotFoundException("ไม่พบหลักสูตรนี้");
        }

        int registeredCount = await CountActiveRegistrationsAsync(shortCourseId, cancellationToken);

        ShortCourseDetailDto dto = new ShortCourseDetailDto();
        dto.Id = shortCourse.Id;
        dto.Title = shortCourse.Title;
        dto.Description = shortCourse.Description;
        dto.Audience = shortCourse.Audience;
        dto.Location = shortCourse.Location;
        dto.TeacherName = shortCourse.TeacherName;
        dto.CoverFileId = shortCourse.CoverFileId;
        dto.StartDate = shortCourse.StartDate;
        dto.EndDate = shortCourse.EndDate;
        dto.Hours = shortCourse.Hours;
        dto.MaxParticipants = shortCourse.MaxParticipants;
        dto.RegisteredCount = registeredCount;
        dto.RegistrationOpenAt = shortCourse.RegistrationOpenAt;
        dto.RegistrationCloseAt = shortCourse.RegistrationCloseAt;
        dto.Availability = ShortCourseAvailabilityCalculator.Calculate(
            shortCourse.IsRegistrationEnabled,
            shortCourse.RegistrationOpenAt,
            shortCourse.RegistrationCloseAt,
            shortCourse.StartDate,
            shortCourse.MaxParticipants,
            registeredCount,
            DateTime.UtcNow);
        return dto;
    }

    public async Task<PagedResult<AdminShortCourseListItemDto>> GetAllAsync(AdminShortCourseQuery query, CancellationToken cancellationToken)
    {
        IQueryable<ShortCourse> shortCourses = _dbContext.ShortCourses.AsNoTracking();

        if (query.IsPublished.HasValue)
        {
            shortCourses = shortCourses.Where(shortCourse => shortCourse.IsPublished == query.IsPublished.Value);
        }

        shortCourses = ApplySearch(shortCourses, query.Search);

        int total = await shortCourses.CountAsync(cancellationToken);

        var rows = await shortCourses
            .OrderByDescending(shortCourse => shortCourse.StartDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(shortCourse => new
            {
                shortCourse.Id,
                shortCourse.Title,
                shortCourse.StartDate,
                shortCourse.EndDate,
                shortCourse.MaxParticipants,
                shortCourse.IsPublished,
                shortCourse.IsRegistrationEnabled,
                shortCourse.RegistrationOpenAt,
                shortCourse.RegistrationCloseAt,
                RegisteredCount = shortCourse.Registrations.Count(registration => registration.Status != RegistrationStatus.Cancelled)
            })
            .ToListAsync(cancellationToken);

        DateTime now = DateTime.UtcNow;
        PagedResult<AdminShortCourseListItemDto> result = new PagedResult<AdminShortCourseListItemDto>();
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;

        foreach (var row in rows)
        {
            AdminShortCourseListItemDto item = new AdminShortCourseListItemDto();
            item.Id = row.Id;
            item.Title = row.Title;
            item.StartDate = row.StartDate;
            item.EndDate = row.EndDate;
            item.MaxParticipants = row.MaxParticipants;
            item.RegisteredCount = row.RegisteredCount;
            item.IsPublished = row.IsPublished;
            item.Availability = ShortCourseAvailabilityCalculator.Calculate(
                row.IsRegistrationEnabled,
                row.RegistrationOpenAt,
                row.RegistrationCloseAt,
                row.StartDate,
                row.MaxParticipants,
                row.RegisteredCount,
                now);
            result.Items.Add(item);
        }

        return result;
    }

    public async Task<AdminShortCourseDto> GetByIdAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        ShortCourse shortCourse = await LoadAsync(shortCourseId, cancellationToken);
        int registeredCount = await CountActiveRegistrationsAsync(shortCourseId, cancellationToken);
        return MapToAdminDto(shortCourse, registeredCount);
    }

    public async Task<AdminShortCourseDto> CreateAsync(SaveShortCourseRequest request, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, 0, cancellationToken);

        ShortCourse shortCourse = new ShortCourse();
        CopyRequestToShortCourse(request, shortCourse);

        _dbContext.ShortCourses.Add(shortCourse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToAdminDto(shortCourse, 0);
    }

    public async Task<AdminShortCourseDto> UpdateAsync(Guid shortCourseId, SaveShortCourseRequest request, CancellationToken cancellationToken)
    {
        ShortCourse shortCourse = await LoadAsync(shortCourseId, cancellationToken);
        int registeredCount = await CountActiveRegistrationsAsync(shortCourseId, cancellationToken);

        await ValidateRequestAsync(request, registeredCount, cancellationToken);

        CopyRequestToShortCourse(request, shortCourse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToAdminDto(shortCourse, registeredCount);
    }

    public async Task<AdminShortCourseDto> SetPublishedAsync(Guid shortCourseId, bool isPublished, CancellationToken cancellationToken)
    {
        ShortCourse shortCourse = await LoadAsync(shortCourseId, cancellationToken);
        shortCourse.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);

        int registeredCount = await CountActiveRegistrationsAsync(shortCourseId, cancellationToken);
        return MapToAdminDto(shortCourse, registeredCount);
    }

    public async Task DeleteAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        ShortCourse shortCourse = await LoadAsync(shortCourseId, cancellationToken);

        int registeredCount = await CountActiveRegistrationsAsync(shortCourseId, cancellationToken);
        if (registeredCount > 0)
        {
            throw new ConflictException("ลบไม่ได้เพราะมีผู้สมัครอยู่ " + registeredCount + " คน ให้ยกเลิกการสมัครทั้งหมดก่อน หรือซ่อนหลักสูตรแทน");
        }

        _dbContext.ShortCourses.Remove(shortCourse);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<ShortCourse> ApplySearch(IQueryable<ShortCourse> shortCourses, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return shortCourses;
        }

        string term = search.Trim();
        return shortCourses.Where(shortCourse => shortCourse.Title.Contains(term));
    }

    private async Task<ShortCourse> LoadAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        ShortCourse? shortCourse = await _dbContext.ShortCourses
            .FirstOrDefaultAsync(item => item.Id == shortCourseId, cancellationToken);
        if (shortCourse == null)
        {
            throw new NotFoundException("ไม่พบหลักสูตรนี้");
        }

        return shortCourse;
    }

    private async Task<int> CountActiveRegistrationsAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        return await _dbContext.ShortCourseRegistrations.CountAsync(
            registration => registration.ShortCourseId == shortCourseId
                && registration.Status != RegistrationStatus.Cancelled,
            cancellationToken);
    }

    private async Task ValidateRequestAsync(SaveShortCourseRequest request, int registeredCount, CancellationToken cancellationToken)
    {
        if (request.EndDate < request.StartDate)
        {
            throw new BadRequestException("วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่มอบรม");
        }

        if (request.RegistrationOpenAt.HasValue
            && request.RegistrationCloseAt.HasValue
            && request.RegistrationCloseAt.Value < request.RegistrationOpenAt.Value)
        {
            throw new BadRequestException("วันปิดรับสมัครต้องไม่ก่อนวันเปิดรับสมัคร");
        }

        if (request.MaxParticipants > 0 && request.MaxParticipants < registeredCount)
        {
            throw new BadRequestException("จำนวนที่รับต้องไม่น้อยกว่าจำนวนผู้สมัครปัจจุบัน (" + registeredCount + " คน)");
        }

        if (request.CoverFileId.HasValue)
        {
            bool coverExists = await _dbContext.Files.AnyAsync(file => file.Id == request.CoverFileId.Value, cancellationToken);
            if (!coverExists)
            {
                throw new BadRequestException("ไม่พบไฟล์รูปปก");
            }
        }

        if (request.CertificateTemplateId.HasValue)
        {
            bool templateExists = await _dbContext.CertificateTemplates
                .AnyAsync(template => template.Id == request.CertificateTemplateId.Value, cancellationToken);
            if (!templateExists)
            {
                throw new BadRequestException("ไม่พบแม่แบบเกียรติบัตร");
            }
        }
    }

    private static void CopyRequestToShortCourse(SaveShortCourseRequest request, ShortCourse shortCourse)
    {
        shortCourse.Title = request.Title.Trim();
        shortCourse.Description = request.Description;
        shortCourse.Audience = request.Audience?.Trim();
        shortCourse.Location = request.Location?.Trim();
        shortCourse.TeacherName = request.TeacherName?.Trim();
        shortCourse.StartDate = request.StartDate;
        shortCourse.EndDate = request.EndDate;
        shortCourse.Hours = request.Hours;
        shortCourse.MaxParticipants = request.MaxParticipants;
        shortCourse.RegistrationOpenAt = request.RegistrationOpenAt;
        shortCourse.RegistrationCloseAt = request.RegistrationCloseAt;
        shortCourse.IsRegistrationEnabled = request.IsRegistrationEnabled;
        shortCourse.IsPublished = request.IsPublished;
        shortCourse.CoverFileId = request.CoverFileId;
        shortCourse.CertificateTemplateId = request.CertificateTemplateId;
    }

    private static AdminShortCourseDto MapToAdminDto(ShortCourse shortCourse, int registeredCount)
    {
        AdminShortCourseDto dto = new AdminShortCourseDto();
        dto.Id = shortCourse.Id;
        dto.Title = shortCourse.Title;
        dto.Description = shortCourse.Description;
        dto.Audience = shortCourse.Audience;
        dto.Location = shortCourse.Location;
        dto.TeacherName = shortCourse.TeacherName;
        dto.StartDate = shortCourse.StartDate;
        dto.EndDate = shortCourse.EndDate;
        dto.Hours = shortCourse.Hours;
        dto.MaxParticipants = shortCourse.MaxParticipants;
        dto.RegisteredCount = registeredCount;
        dto.RegistrationOpenAt = shortCourse.RegistrationOpenAt;
        dto.RegistrationCloseAt = shortCourse.RegistrationCloseAt;
        dto.IsRegistrationEnabled = shortCourse.IsRegistrationEnabled;
        dto.IsPublished = shortCourse.IsPublished;
        dto.CoverFileId = shortCourse.CoverFileId;
        dto.CertificateTemplateId = shortCourse.CertificateTemplateId;
        return dto;
    }
}
