using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.TechSubjects;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class TechSubjectService : ITechSubjectService
{
    private readonly AppDbContext _dbContext;

    public TechSubjectService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TechSubjectListItemDto>> GetPublishedAsync(CancellationToken cancellationToken)
    {
        List<TechSubjectListItemDto> subjects = await _dbContext.TechSubjects
            .AsNoTracking()
            .Where(subject => subject.IsPublished)
            .OrderBy(subject => subject.SortOrder)
            .ThenBy(subject => subject.SubjectName)
            .ThenBy(subject => subject.Id)
            .Select(subject => new TechSubjectListItemDto
            {
                Id = subject.Id,
                Icon = subject.Icon,
                SubjectName = subject.SubjectName,
                SubTitle = subject.SubTitle,
                MainImageFileId = subject.Files
                    .OrderBy(file => file.SortOrder)
                    .Select(file => (Guid?)file.FileId)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return subjects;
    }

    public async Task<TechSubjectDetailDto> GetPublishedByIdAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        TechSubjectDetailDto? detail = await _dbContext.TechSubjects
            .AsNoTracking()
            .Where(subject => subject.Id == subjectId && subject.IsPublished)
            .Select(subject => new TechSubjectDetailDto
            {
                Id = subject.Id,
                Icon = subject.Icon,
                SubjectName = subject.SubjectName,
                SubTitle = subject.SubTitle,
                Description = subject.Description,
                ImageFileIds = subject.Files
                    .OrderBy(file => file.SortOrder)
                    .Select(file => file.FileId)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail == null)
        {
            throw new NotFoundException("Subject not found.");
        }

        return detail;
    }

    public async Task<List<AdminTechSubjectDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<AdminTechSubjectDto> subjects = await SelectAdminDtos(_dbContext.TechSubjects.AsNoTracking())
            .ToListAsync(cancellationToken);

        return subjects;
    }

    public async Task<AdminTechSubjectDto> GetByIdAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        IQueryable<TechSubject> query = _dbContext.TechSubjects
            .AsNoTracking()
            .Where(subject => subject.Id == subjectId);

        AdminTechSubjectDto? dto = await SelectAdminDtos(query).FirstOrDefaultAsync(cancellationToken);
        if (dto == null)
        {
            throw new NotFoundException("Subject not found.");
        }

        return dto;
    }

    public async Task<AdminTechSubjectDto> CreateAsync(SaveTechSubjectRequest request, CancellationToken cancellationToken)
    {
        List<Guid> fileIds = await GetValidatedFileIdsAsync(request, cancellationToken);

        TechSubject subject = new TechSubject();
        CopyRequestToSubject(request, subject);
        AddFiles(subject, fileIds);

        _dbContext.TechSubjects.Add(subject);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(subject.Id, cancellationToken);
    }

    public async Task<AdminTechSubjectDto> UpdateAsync(Guid subjectId, SaveTechSubjectRequest request, CancellationToken cancellationToken)
    {
        TechSubject? subject = await _dbContext.TechSubjects
            .Include(item => item.Files)
            .FirstOrDefaultAsync(item => item.Id == subjectId, cancellationToken);
        if (subject == null)
        {
            throw new NotFoundException("Subject not found.");
        }

        List<Guid> fileIds = await GetValidatedFileIdsAsync(request, cancellationToken);

        CopyRequestToSubject(request, subject);

        // ห้าม subject.Files.Clear() และต้อง Add ที่ DbSet ตรงๆ (เหตุผลเดียวกับรูปของข่าว)
        _dbContext.TechSubjectFiles.RemoveRange(subject.Files.ToList());
        AddFiles(subject, fileIds);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(subjectId, cancellationToken);
    }

    public async Task<AdminTechSubjectDto> SetPublishedAsync(Guid subjectId, bool isPublished, CancellationToken cancellationToken)
    {
        TechSubject subject = await LoadSubjectAsync(subjectId, cancellationToken);

        subject.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(subjectId, cancellationToken);
    }

    public async Task MoveAsync(Guid subjectId, bool moveUp, CancellationToken cancellationToken)
    {
        List<TechSubject> orderedSubjects = await _dbContext.TechSubjects
            .OrderBy(subject => subject.SortOrder)
            .ThenBy(subject => subject.SubjectName)
            .ThenBy(subject => subject.Id)
            .ToListAsync(cancellationToken);

        int currentIndex = orderedSubjects.FindIndex(subject => subject.Id == subjectId);
        if (currentIndex < 0)
        {
            throw new NotFoundException("Subject not found.");
        }

        int targetIndex = moveUp ? currentIndex - 1 : currentIndex + 1;
        if (targetIndex >= 0 && targetIndex < orderedSubjects.Count)
        {
            (orderedSubjects[currentIndex], orderedSubjects[targetIndex]) = (orderedSubjects[targetIndex], orderedSubjects[currentIndex]);
        }

        for (int position = 0; position < orderedSubjects.Count; position++)
        {
            orderedSubjects[position].SortOrder = position;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        // โหลดเฉพาะการ์ด ไม่รวมรูป ไม่งั้น soft delete จะชนกับแถวที่ EF ติดตามอยู่
        TechSubject subject = await LoadSubjectAsync(subjectId, cancellationToken);

        _dbContext.TechSubjects.Remove(subject);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<AdminTechSubjectDto> SelectAdminDtos(IQueryable<TechSubject> query)
    {
        return query
            .OrderBy(subject => subject.SortOrder)
            .ThenBy(subject => subject.SubjectName)
            .ThenBy(subject => subject.Id)
            .Select(subject => new AdminTechSubjectDto
            {
                Id = subject.Id,
                Icon = subject.Icon,
                SubjectName = subject.SubjectName,
                SubTitle = subject.SubTitle,
                Description = subject.Description,
                SortOrder = subject.SortOrder,
                IsPublished = subject.IsPublished,
                ImageFileIds = subject.Files
                    .OrderBy(file => file.SortOrder)
                    .Select(file => file.FileId)
                    .ToList(),
                CreatedAt = subject.CreatedAt
            });
    }

    private async Task<TechSubject> LoadSubjectAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        TechSubject? subject = await _dbContext.TechSubjects
            .FirstOrDefaultAsync(item => item.Id == subjectId, cancellationToken);
        if (subject == null)
        {
            throw new NotFoundException("Subject not found.");
        }

        return subject;
    }

    private async Task<List<Guid>> GetValidatedFileIdsAsync(SaveTechSubjectRequest request, CancellationToken cancellationToken)
    {
        List<Guid> fileIds = new List<Guid>();
        if (request.FileIds != null)
        {
            fileIds = request.FileIds.Distinct().ToList();
        }

        if (fileIds.Count > 0)
        {
            int existingCount = await _dbContext.Files.CountAsync(file => fileIds.Contains(file.Id), cancellationToken);
            if (existingCount != fileIds.Count)
            {
                throw new BadRequestException("Some files do not exist.");
            }
        }

        return fileIds;
    }

    private static void CopyRequestToSubject(SaveTechSubjectRequest request, TechSubject subject)
    {
        subject.Icon = request.Icon?.Trim();
        subject.SubjectName = request.SubjectName.Trim();
        subject.SubTitle = request.SubTitle?.Trim();
        subject.Description = request.Description;
        subject.SortOrder = request.SortOrder;
        subject.IsPublished = request.IsPublished;
    }

    private void AddFiles(TechSubject subject, List<Guid> fileIds)
    {
        for (int index = 0; index < fileIds.Count; index++)
        {
            TechSubjectFile subjectFile = new TechSubjectFile();
            subjectFile.TechSubjectId = subject.Id;
            subjectFile.FileId = fileIds[index];
            subjectFile.SortOrder = index;
            _dbContext.TechSubjectFiles.Add(subjectFile);
        }
    }
}
