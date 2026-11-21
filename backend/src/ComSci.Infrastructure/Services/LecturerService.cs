using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Lecturers;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class LecturerService : ILecturerService
{
    private readonly AppDbContext _dbContext;

    public LecturerService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<LecturerDto>> GetPublishedAsync(CancellationToken cancellationToken)
    {
        List<LecturerDto> lecturers = await _dbContext.Lecturers
            .AsNoTracking()
            .Where(lecturer => lecturer.IsPublished)
            .OrderBy(lecturer => lecturer.SortOrder)
            .ThenBy(lecturer => lecturer.FullName)
            .Select(lecturer => new LecturerDto
            {
                Id = lecturer.Id,
                Prefix = lecturer.Prefix,
                FullName = lecturer.FullName,
                Position = lecturer.Position,
                Expertise = lecturer.Expertise,
                EducationLevel = lecturer.EducationLevel,
                Program = lecturer.Program,
                ImageFileId = lecturer.ImageFileId
            })
            .ToListAsync(cancellationToken);

        return lecturers;
    }

    public async Task<PagedResult<AdminLecturerDto>> GetAllAsync(AdminLecturerQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Lecturer> lecturerQuery = _dbContext.Lecturers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string keyword = query.Search.Trim();
            lecturerQuery = lecturerQuery.Where(lecturer => lecturer.FullName.Contains(keyword));
        }

        if (query.IsPublished.HasValue)
        {
            bool wantedState = query.IsPublished.Value;
            lecturerQuery = lecturerQuery.Where(lecturer => lecturer.IsPublished == wantedState);
        }

        int total = await lecturerQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<AdminLecturerDto> items = await SelectAdminDtos(ApplyAdminOrdering(lecturerQuery, query.SortBy, query.SortDescending))
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        PagedResult<AdminLecturerDto> result = new PagedResult<AdminLecturerDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<AdminLecturerDto> GetByIdAsync(Guid lecturerId, CancellationToken cancellationToken)
    {
        IQueryable<Lecturer> lecturerQuery = _dbContext.Lecturers
            .AsNoTracking()
            .Where(lecturer => lecturer.Id == lecturerId);

        AdminLecturerDto? dto = await SelectAdminDtos(lecturerQuery).FirstOrDefaultAsync(cancellationToken);
        if (dto == null)
        {
            throw new NotFoundException("Lecturer not found.");
        }

        return dto;
    }

    public async Task<AdminLecturerDto> CreateAsync(SaveLecturerRequest request, CancellationToken cancellationToken)
    {
        await EnsureImageFileExistsAsync(request.ImageFileId, cancellationToken);

        Lecturer lecturer = new Lecturer();
        CopyRequestToLecturer(request, lecturer);

        _dbContext.Lecturers.Add(lecturer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(lecturer.Id, cancellationToken);
    }

    public async Task<AdminLecturerDto> UpdateAsync(Guid lecturerId, SaveLecturerRequest request, CancellationToken cancellationToken)
    {
        Lecturer lecturer = await LoadLecturerAsync(lecturerId, cancellationToken);
        await EnsureImageFileExistsAsync(request.ImageFileId, cancellationToken);

        CopyRequestToLecturer(request, lecturer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(lecturerId, cancellationToken);
    }

    public async Task<AdminLecturerDto> SetPublishedAsync(Guid lecturerId, bool isPublished, CancellationToken cancellationToken)
    {
        Lecturer lecturer = await LoadLecturerAsync(lecturerId, cancellationToken);

        lecturer.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(lecturerId, cancellationToken);
    }

    public async Task MoveAsync(Guid lecturerId, bool moveUp, CancellationToken cancellationToken)
    {
        List<Lecturer> orderedLecturers = await _dbContext.Lecturers
            .OrderBy(lecturer => lecturer.SortOrder)
            .ThenBy(lecturer => lecturer.FullName)
            .ThenBy(lecturer => lecturer.Id)
            .ToListAsync(cancellationToken);

        int currentIndex = orderedLecturers.FindIndex(lecturer => lecturer.Id == lecturerId);
        if (currentIndex < 0)
        {
            throw new NotFoundException("Lecturer not found.");
        }

        int targetIndex = moveUp ? currentIndex - 1 : currentIndex + 1;
        if (targetIndex >= 0 && targetIndex < orderedLecturers.Count)
        {
            (orderedLecturers[currentIndex], orderedLecturers[targetIndex]) = (orderedLecturers[targetIndex], orderedLecturers[currentIndex]);
        }

        for (int position = 0; position < orderedLecturers.Count; position++)
        {
            orderedLecturers[position].SortOrder = position;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid lecturerId, CancellationToken cancellationToken)
    {
        Lecturer lecturer = await LoadLecturerAsync(lecturerId, cancellationToken);

        // AuditSaveChangesInterceptor เปลี่ยนคำสั่งลบนี้เป็น soft delete
        _dbContext.Lecturers.Remove(lecturer);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Lecturer> ApplyAdminOrdering(IQueryable<Lecturer> lecturerQuery, string? sortBy, bool descending)
    {
        IOrderedQueryable<Lecturer> ordered = (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("fullname", false) => lecturerQuery.OrderBy(lecturer => lecturer.FullName),
            ("fullname", true) => lecturerQuery.OrderByDescending(lecturer => lecturer.FullName),
            ("position", false) => lecturerQuery.OrderBy(lecturer => lecturer.Position),
            ("position", true) => lecturerQuery.OrderByDescending(lecturer => lecturer.Position),
            ("sortorder", false) => lecturerQuery.OrderBy(lecturer => lecturer.SortOrder),
            ("sortorder", true) => lecturerQuery.OrderByDescending(lecturer => lecturer.SortOrder),
            ("status", false) => lecturerQuery.OrderBy(lecturer => lecturer.IsPublished),
            ("status", true) => lecturerQuery.OrderByDescending(lecturer => lecturer.IsPublished),
            _ => lecturerQuery.OrderBy(lecturer => lecturer.SortOrder)
        };

        return ordered.ThenBy(lecturer => lecturer.FullName).ThenBy(lecturer => lecturer.Id);
    }

    private static IQueryable<AdminLecturerDto> SelectAdminDtos(IQueryable<Lecturer> lecturerQuery)
    {
        return lecturerQuery
            .Select(lecturer => new AdminLecturerDto
            {
                Id = lecturer.Id,
                Prefix = lecturer.Prefix,
                FullName = lecturer.FullName,
                Position = lecturer.Position,
                Expertise = lecturer.Expertise,
                EducationLevel = lecturer.EducationLevel,
                Program = lecturer.Program,
                Contact = lecturer.Contact,
                ImageFileId = lecturer.ImageFileId,
                SortOrder = lecturer.SortOrder,
                IsPublished = lecturer.IsPublished,
                CreatedAt = lecturer.CreatedAt
            });
    }

    private async Task<Lecturer> LoadLecturerAsync(Guid lecturerId, CancellationToken cancellationToken)
    {
        Lecturer? lecturer = await _dbContext.Lecturers
            .FirstOrDefaultAsync(item => item.Id == lecturerId, cancellationToken);
        if (lecturer == null)
        {
            throw new NotFoundException("Lecturer not found.");
        }

        return lecturer;
    }

    private async Task EnsureImageFileExistsAsync(Guid? imageFileId, CancellationToken cancellationToken)
    {
        if (!imageFileId.HasValue)
        {
            return;
        }

        bool exists = await _dbContext.Files.AnyAsync(file => file.Id == imageFileId.Value, cancellationToken);
        if (!exists)
        {
            throw new BadRequestException("Image file does not exist.");
        }
    }

    private static void CopyRequestToLecturer(SaveLecturerRequest request, Lecturer lecturer)
    {
        lecturer.Prefix = request.Prefix?.Trim();
        lecturer.FullName = request.FullName.Trim();
        lecturer.Position = request.Position?.Trim();
        lecturer.Expertise = request.Expertise?.Trim();
        lecturer.EducationLevel = request.EducationLevel?.Trim();
        lecturer.Program = request.Program?.Trim();
        lecturer.Contact = request.Contact?.Trim();
        lecturer.ImageFileId = request.ImageFileId;
        lecturer.SortOrder = request.SortOrder;
        lecturer.IsPublished = request.IsPublished;
    }
}
