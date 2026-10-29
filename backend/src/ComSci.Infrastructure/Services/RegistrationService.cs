using System.Data;
using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Courses;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ComSci.Infrastructure.Services;

public class RegistrationService : IRegistrationService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public RegistrationService(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<MyRegistrationDto> RegisterAsync(Guid generationId, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();

        // Serializable: นับที่นั่งกับเพิ่มแถวต้องเป็นขั้นตอนเดียวกัน ไม่งั้นคนสองคนที่กดพร้อมกันอาจได้ที่นั่งสุดท้ายทั้งคู่
        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken))
        {
            Generation? generation = await _dbContext.Generations
                .FirstOrDefaultAsync(item => item.Id == generationId && item.Course.IsPublished, cancellationToken);
            if (generation == null)
            {
                throw new NotFoundException("Generation not found.");
            }

            if (generation.IsCancelled)
            {
                throw new BadRequestException("This generation has been cancelled.");
            }

            if (generation.StartDate <= DateTime.UtcNow)
            {
                throw new BadRequestException("Registration is closed because the generation has already started.");
            }

            CourseRegistration? existing = await _dbContext.CourseRegistrations
                .FirstOrDefaultAsync(item => item.GenerationId == generationId && item.UserId == userId, cancellationToken);
            if (existing != null && existing.Status != RegistrationStatus.Cancelled)
            {
                throw new ConflictException("You are already registered for this generation.");
            }

            int registeredCount = await _dbContext.CourseRegistrations.CountAsync(
                item => item.GenerationId == generationId && item.Status != RegistrationStatus.Cancelled,
                cancellationToken);
            if (registeredCount >= generation.Quantity)
            {
                throw new ConflictException("This generation is full.");
            }

            if (existing != null)
            {
                // เคยยกเลิกไปแล้ว ลงทะเบียนใหม่ได้ ใช้แถวเดิมเพราะคีย์หลักคือ UserId + GenerationId
                existing.Status = RegistrationStatus.Confirmed;
                existing.RegisteredAt = DateTime.UtcNow;
                existing.CancelledAt = null;
            }
            else
            {
                CourseRegistration registration = new CourseRegistration();
                registration.UserId = userId;
                registration.GenerationId = generationId;
                registration.Status = RegistrationStatus.Confirmed;
                registration.RegisteredAt = DateTime.UtcNow;
                _dbContext.CourseRegistrations.Add(registration);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        return await GetMyRegistrationAsync(generationId, userId, cancellationToken);
    }

    public async Task CancelMyRegistrationAsync(Guid generationId, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();
        await CancelRegistrationAsync(generationId, userId, cancellationToken);
    }

    public async Task<List<MyRegistrationDto>> GetMyRegistrationsAsync(CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();

        List<MyRegistrationDto> registrations = await SelectMyRegistrations(userId)
            .OrderByDescending(registration => registration.StartDate)
            .ToListAsync(cancellationToken);

        return registrations;
    }

    public async Task<List<RegistrationListItemDto>> GetGenerationRegistrationsAsync(Guid generationId, CancellationToken cancellationToken)
    {
        bool generationExists = await _dbContext.Generations.AnyAsync(item => item.Id == generationId, cancellationToken);
        if (!generationExists)
        {
            throw new NotFoundException("Generation not found.");
        }

        List<RegistrationListItemDto> registrations = await _dbContext.CourseRegistrations
            .AsNoTracking()
            .Where(registration => registration.GenerationId == generationId)
            .OrderBy(registration => registration.RegisteredAt)
            .Select(registration => new RegistrationListItemDto
            {
                UserId = registration.UserId,
                FullName = registration.User.FullName,
                Email = registration.User.Email ?? string.Empty,
                Status = registration.Status,
                RegisteredAt = registration.RegisteredAt,
                CancelledAt = registration.CancelledAt
            })
            .ToListAsync(cancellationToken);

        return registrations;
    }

    public async Task CancelRegistrationAsync(Guid generationId, Guid userId, CancellationToken cancellationToken)
    {
        CourseRegistration? registration = await _dbContext.CourseRegistrations
            .FirstOrDefaultAsync(
                item => item.GenerationId == generationId
                    && item.UserId == userId
                    && item.Status != RegistrationStatus.Cancelled,
                cancellationToken);
        if (registration == null)
        {
            throw new NotFoundException("Registration not found.");
        }

        registration.Status = RegistrationStatus.Cancelled;
        registration.CancelledAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No authenticated user.");
        }

        return _currentUser.UserId.Value;
    }

    private IQueryable<MyRegistrationDto> SelectMyRegistrations(Guid userId)
    {
        return _dbContext.CourseRegistrations
            .AsNoTracking()
            .Where(registration => registration.UserId == userId)
            .Select(registration => new MyRegistrationDto
            {
                GenerationId = registration.GenerationId,
                CourseId = registration.Generation.CourseId,
                CourseTitle = registration.Generation.Course.Title,
                GenerationName = registration.Generation.SubTitle,
                StartDate = registration.Generation.StartDate,
                EndDate = registration.Generation.EndDate,
                Status = registration.Status,
                RegisteredAt = registration.RegisteredAt
            });
    }

    private async Task<MyRegistrationDto> GetMyRegistrationAsync(Guid generationId, Guid userId, CancellationToken cancellationToken)
    {
        MyRegistrationDto? dto = await SelectMyRegistrations(userId)
            .FirstOrDefaultAsync(registration => registration.GenerationId == generationId, cancellationToken);
        if (dto == null)
        {
            throw new NotFoundException("Registration not found.");
        }

        return dto;
    }
}
