using System.Data;
using System.Security.Cryptography;
using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.ShortCourses;
using ComSci.Application.Exceptions;
using ComSci.Application.Helpers;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ComSci.Infrastructure.Services;

public class ShortCourseRegistrationService : IShortCourseRegistrationService
{
    private const string CodePrefix = "SC-";
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 6;
    private const int MaximumCodeAttempts = 10;
    private const string RegistrationNotFoundMessage = "ไม่พบข้อมูลการสมัคร กรุณาตรวจสอบรหัสการสมัครและเบอร์โทรอีกครั้ง";

    private readonly AppDbContext _dbContext;

    public ShortCourseRegistrationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShortCourseRegistrationDto> RegisterAsync(Guid shortCourseId, RegisterShortCourseRequest request, CancellationToken cancellationToken)
    {
        string? phone = PhoneNumberNormalizer.Normalize(request.Phone);
        if (phone == null)
        {
            throw new BadRequestException("เบอร์โทรไม่ถูกต้อง ต้องเป็นตัวเลข 9-10 หลักและขึ้นต้นด้วย 0");
        }

        string firstName = request.FirstName.Trim();
        string lastName = request.LastName.Trim();
        if (firstName.Length == 0 || lastName.Length == 0)
        {
            throw new BadRequestException("กรุณากรอกชื่อและนามสกุล");
        }

        ShortCourseRegistration registration = new ShortCourseRegistration();

        // Serializable: นับที่นั่งกับเพิ่มแถวต้องเป็นขั้นตอนเดียวกัน ไม่งั้นคนสองคนที่กดพร้อมกันอาจได้ที่นั่งสุดท้ายทั้งคู่
        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken))
        {
            ShortCourse? shortCourse = await _dbContext.ShortCourses
                .FirstOrDefaultAsync(item => item.Id == shortCourseId && item.IsPublished, cancellationToken);
            if (shortCourse == null)
            {
                throw new NotFoundException("ไม่พบหลักสูตรนี้");
            }

            int registeredCount = await _dbContext.ShortCourseRegistrations.CountAsync(
                item => item.ShortCourseId == shortCourseId && item.Status != RegistrationStatus.Cancelled,
                cancellationToken);

            ShortCourseAvailability availability = ShortCourseAvailabilityCalculator.Calculate(
                shortCourse.IsRegistrationEnabled,
                shortCourse.RegistrationOpenAt,
                shortCourse.RegistrationCloseAt,
                shortCourse.StartDate,
                shortCourse.MaxParticipants,
                registeredCount,
                DateTime.UtcNow);

            if (availability == ShortCourseAvailability.NotOpenYet)
            {
                throw new BadRequestException("หลักสูตรนี้ยังไม่เปิดรับสมัคร");
            }

            if (availability == ShortCourseAvailability.Closed)
            {
                throw new BadRequestException("หลักสูตรนี้ปิดรับสมัครแล้ว");
            }

            if (availability == ShortCourseAvailability.Full)
            {
                throw new ConflictException("จำนวนผู้สมัครเต็มแล้ว");
            }

            bool alreadyRegistered = await _dbContext.ShortCourseRegistrations.AnyAsync(
                item => item.ShortCourseId == shortCourseId
                    && item.Phone == phone
                    && item.Status != RegistrationStatus.Cancelled,
                cancellationToken);
            if (alreadyRegistered)
            {
                throw new ConflictException("เบอร์โทรนี้สมัครหลักสูตรนี้ไว้แล้ว หากลืมรหัสการสมัคร กรุณาติดต่อเจ้าหน้าที่");
            }

            registration.ShortCourseId = shortCourseId;
            registration.Code = await GenerateUniqueCodeAsync(cancellationToken);
            registration.Prefix = string.IsNullOrWhiteSpace(request.Prefix) ? null : request.Prefix.Trim();
            registration.FirstName = firstName;
            registration.LastName = lastName;
            registration.Phone = phone;
            registration.Status = RegistrationStatus.Confirmed;
            registration.RegisteredAt = DateTime.UtcNow;

            _dbContext.ShortCourseRegistrations.Add(registration);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        return await GetOwnedRegistrationAsync(registration.Code, phone, cancellationToken);
    }

    public async Task<ShortCourseRegistrationDto> LookupAsync(LookupShortCourseRegistrationRequest request, CancellationToken cancellationToken)
    {
        string? phone = PhoneNumberNormalizer.Normalize(request.Phone);
        if (phone == null)
        {
            throw new NotFoundException(RegistrationNotFoundMessage);
        }

        return await GetOwnedRegistrationAsync(NormalizeCode(request.Code), phone, cancellationToken);
    }

    public async Task<ShortCourseRegistrationDto> UpdateAsync(string code, UpdateShortCourseRegistrationRequest request, CancellationToken cancellationToken)
    {
        ShortCourseRegistration registration = await LoadOwnedRegistrationAsync(NormalizeCode(code), request.CurrentPhone, cancellationToken);
        EnsureCanModify(registration);

        string firstName = request.FirstName.Trim();
        string lastName = request.LastName.Trim();
        if (firstName.Length == 0 || lastName.Length == 0)
        {
            throw new BadRequestException("กรุณากรอกชื่อและนามสกุล");
        }

        registration.Prefix = string.IsNullOrWhiteSpace(request.Prefix) ? null : request.Prefix.Trim();
        registration.FirstName = firstName;
        registration.LastName = lastName;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetOwnedRegistrationAsync(registration.Code, registration.Phone, cancellationToken);
    }

    public async Task CancelAsync(string code, CancelShortCourseRegistrationRequest request, CancellationToken cancellationToken)
    {
        ShortCourseRegistration registration = await LoadOwnedRegistrationAsync(NormalizeCode(code), request.Phone, cancellationToken);
        EnsureCanModify(registration);

        registration.Status = RegistrationStatus.Cancelled;
        registration.CancelledAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AdminShortCourseRegistrationDto>> GetByShortCourseAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        bool shortCourseExists = await _dbContext.ShortCourses.AnyAsync(item => item.Id == shortCourseId, cancellationToken);
        if (!shortCourseExists)
        {
            throw new NotFoundException("ไม่พบหลักสูตรนี้");
        }

        List<AdminShortCourseRegistrationDto> registrations = await _dbContext.ShortCourseRegistrations
            .AsNoTracking()
            .Where(registration => registration.ShortCourseId == shortCourseId)
            .OrderBy(registration => registration.RegisteredAt)
            .Select(registration => new AdminShortCourseRegistrationDto
            {
                Id = registration.Id,
                Code = registration.Code,
                Prefix = registration.Prefix,
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Phone = registration.Phone,
                Status = registration.Status,
                RegisteredAt = registration.RegisteredAt,
                CancelledAt = registration.CancelledAt
            })
            .ToListAsync(cancellationToken);

        return registrations;
    }

    public async Task CancelByAdminAsync(Guid shortCourseId, Guid registrationId, CancellationToken cancellationToken)
    {
        ShortCourseRegistration? registration = await _dbContext.ShortCourseRegistrations
            .FirstOrDefaultAsync(
                item => item.Id == registrationId
                    && item.ShortCourseId == shortCourseId
                    && item.Status != RegistrationStatus.Cancelled,
                cancellationToken);
        if (registration == null)
        {
            throw new NotFoundException("ไม่พบข้อมูลการสมัคร");
        }

        registration.Status = RegistrationStatus.Cancelled;
        registration.CancelledAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    private static void EnsureCanModify(ShortCourseRegistration registration)
    {
        if (registration.Status == RegistrationStatus.Cancelled)
        {
            throw new BadRequestException("การสมัครนี้ถูกยกเลิกไปแล้ว");
        }

        if (!CanModify(registration))
        {
            throw new BadRequestException("หมดเขตรับสมัครแล้ว ไม่สามารถแก้ไขหรือยกเลิกได้ กรุณาติดต่อเจ้าหน้าที่");
        }
    }

    private static bool CanModify(ShortCourseRegistration registration)
    {
        if (registration.Status == RegistrationStatus.Cancelled)
        {
            return false;
        }

        DateTime deadline = ShortCourseAvailabilityCalculator.GetRegistrationDeadline(
            registration.ShortCourse.RegistrationCloseAt,
            registration.ShortCourse.StartDate);
        return DateTime.UtcNow < deadline;
    }

    private async Task<ShortCourseRegistration> LoadOwnedRegistrationAsync(string code, string phoneInput, CancellationToken cancellationToken)
    {
        string? phone = PhoneNumberNormalizer.Normalize(phoneInput);
        if (phone == null)
        {
            throw new NotFoundException(RegistrationNotFoundMessage);
        }

        ShortCourseRegistration? registration = await _dbContext.ShortCourseRegistrations
            .Include(item => item.ShortCourse)
            .FirstOrDefaultAsync(item => item.Code == code && item.Phone == phone, cancellationToken);
        if (registration == null)
        {
            throw new NotFoundException(RegistrationNotFoundMessage);
        }

        return registration;
    }

    private async Task<ShortCourseRegistrationDto> GetOwnedRegistrationAsync(string code, string phone, CancellationToken cancellationToken)
    {
        ShortCourseRegistration registration = await LoadOwnedRegistrationAsync(code, phone, cancellationToken);

        ShortCourseRegistrationDto dto = new ShortCourseRegistrationDto();
        dto.Id = registration.Id;
        dto.Code = registration.Code;
        dto.ShortCourseId = registration.ShortCourseId;
        dto.CourseTitle = registration.ShortCourse.Title;
        dto.StartDate = registration.ShortCourse.StartDate;
        dto.EndDate = registration.ShortCourse.EndDate;
        dto.Prefix = registration.Prefix;
        dto.FirstName = registration.FirstName;
        dto.LastName = registration.LastName;
        dto.Phone = registration.Phone;
        dto.Status = registration.Status;
        dto.RegisteredAt = registration.RegisteredAt;
        dto.CanModify = CanModify(registration);
        return dto;
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < MaximumCodeAttempts; attempt++)
        {
            char[] characters = new char[CodeLength];
            for (int index = 0; index < CodeLength; index++)
            {
                characters[index] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
            }

            string code = CodePrefix + new string(characters);
            bool exists = await _dbContext.ShortCourseRegistrations
                .IgnoreQueryFilters()
                .AnyAsync(item => item.Code == code, cancellationToken);
            if (!exists)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Could not generate a unique registration code.");
    }
}
