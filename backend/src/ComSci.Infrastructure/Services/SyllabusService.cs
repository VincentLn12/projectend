using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Syllabuses;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class SyllabusService : ISyllabusService
{
    private readonly AppDbContext _dbContext;

    public SyllabusService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SyllabusListItemDto>> GetPublishedAsync(CancellationToken cancellationToken)
    {
        List<SyllabusListItemDto> syllabuses = await _dbContext.Syllabuses
            .AsNoTracking()
            .Where(syllabus => syllabus.IsPublished)
            .OrderByDescending(syllabus => syllabus.Year)
            .Select(syllabus => new SyllabusListItemDto
            {
                Id = syllabus.Id,
                NameTH = syllabus.NameTH,
                NameEN = syllabus.NameEN,
                Year = syllabus.Year,
                DegreeTH = syllabus.DegreeTH,
                DegreeEN = syllabus.DegreeEN,
                TotalCredits = syllabus.TotalCredits,
                PdfFileId = syllabus.PdfFileId,
                IsCurrent = syllabus.IsCurrent
            })
            .ToListAsync(cancellationToken);

        return syllabuses;
    }

    public async Task<SyllabusDetailDto> GetPublishedByIdAsync(Guid syllabusId, CancellationToken cancellationToken)
    {
        IQueryable<Syllabus> query = _dbContext.Syllabuses
            .AsNoTracking()
            .Where(syllabus => syllabus.Id == syllabusId && syllabus.IsPublished);

        return await SelectDetail(query, cancellationToken);
    }

    public async Task<SyllabusDetailDto> GetCurrentAsync(CancellationToken cancellationToken)
    {
        IQueryable<Syllabus> query = _dbContext.Syllabuses
            .AsNoTracking()
            .Where(syllabus => syllabus.IsCurrent && syllabus.IsPublished);

        return await SelectDetail(query, cancellationToken);
    }

    public async Task<PagedResult<AdminSyllabusListItemDto>> GetAllAsync(AdminSyllabusQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Syllabus> syllabusQuery = _dbContext.Syllabuses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string keyword = query.Search.Trim();
            syllabusQuery = syllabusQuery.Where(syllabus =>
                syllabus.NameTH.Contains(keyword)
                || (syllabus.NameEN != null && syllabus.NameEN.Contains(keyword)));
        }

        if (query.IsPublished.HasValue)
        {
            bool wantedState = query.IsPublished.Value;
            syllabusQuery = syllabusQuery.Where(syllabus => syllabus.IsPublished == wantedState);
        }

        int total = await syllabusQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<AdminSyllabusListItemDto> items = await syllabusQuery
            .OrderByDescending(syllabus => syllabus.Year)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(syllabus => new AdminSyllabusListItemDto
            {
                Id = syllabus.Id,
                NameTH = syllabus.NameTH,
                Year = syllabus.Year,
                IsCurrent = syllabus.IsCurrent,
                IsPublished = syllabus.IsPublished,
                CreatedAt = syllabus.CreatedAt
            })
            .ToListAsync(cancellationToken);

        PagedResult<AdminSyllabusListItemDto> result = new PagedResult<AdminSyllabusListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<AdminSyllabusDto> GetByIdAsync(Guid syllabusId, CancellationToken cancellationToken)
    {
        AdminSyllabusDto? dto = await _dbContext.Syllabuses
            .AsNoTracking()
            .Where(syllabus => syllabus.Id == syllabusId)
            .Select(syllabus => new AdminSyllabusDto
            {
                Id = syllabus.Id,
                NameTH = syllabus.NameTH,
                NameEN = syllabus.NameEN,
                Year = syllabus.Year,
                DegreeTH = syllabus.DegreeTH,
                DegreeEN = syllabus.DegreeEN,
                TotalCredits = syllabus.TotalCredits,
                PdfFileId = syllabus.PdfFileId,
                IsCurrent = syllabus.IsCurrent,
                IsPublished = syllabus.IsPublished,
                CreatedAt = syllabus.CreatedAt,
                UpdatedAt = syllabus.UpdatedAt,
                Objectives = syllabus.Objectives
                    .OrderBy(objective => objective.SortOrder)
                    .Select(objective => objective.Sentence)
                    .ToList(),
                Occupations = syllabus.Occupations
                    .OrderBy(occupation => occupation.SortOrder)
                    .Select(occupation => occupation.Sentence)
                    .ToList(),
                Subjects = syllabus.Subjects
                    .OrderBy(subject => subject.SubjectCategory)
                    .ThenBy(subject => subject.Code)
                    .ThenBy(subject => subject.Name)
                    .Select(subject => new SubjectDto
                    {
                        Id = subject.Id,
                        Code = subject.Code,
                        Name = subject.Name,
                        Credit = subject.Credit,
                        SubjectCategory = subject.SubjectCategory
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw new NotFoundException("Syllabus not found.");
        }

        return dto;
    }

    public async Task<AdminSyllabusDto> CreateAsync(SaveSyllabusRequest request, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);

        Syllabus syllabus = new Syllabus();
        CopyRequestToSyllabus(request, syllabus);
        AddChildren(syllabus, request);
        await ClearCurrentOnOthersAsync(syllabus, cancellationToken);

        _dbContext.Syllabuses.Add(syllabus);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(syllabus.Id, cancellationToken);
    }

    public async Task<AdminSyllabusDto> UpdateAsync(Guid syllabusId, SaveSyllabusRequest request, CancellationToken cancellationToken)
    {
        Syllabus? syllabus = await _dbContext.Syllabuses
            .Include(item => item.Objectives)
            .Include(item => item.Occupations)
            .Include(item => item.Subjects)
            .FirstOrDefaultAsync(item => item.Id == syllabusId, cancellationToken);
        if (syllabus == null)
        {
            throw new NotFoundException("Syllabus not found.");
        }

        await ValidateRequestAsync(request, cancellationToken);

        CopyRequestToSyllabus(request, syllabus);

        // ห้ามเรียก .Clear() ของ collection และต้อง Add ที่ DbSet ตรงๆ (เหตุผลเดียวกับรูปของข่าว)
        _dbContext.SyllabusObjectives.RemoveRange(syllabus.Objectives.ToList());
        _dbContext.SyllabusOccupations.RemoveRange(syllabus.Occupations.ToList());
        _dbContext.Subjects.RemoveRange(syllabus.Subjects.ToList());
        AddChildren(syllabus, request);

        await ClearCurrentOnOthersAsync(syllabus, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(syllabusId, cancellationToken);
    }

    public async Task<AdminSyllabusDto> SetPublishedAsync(Guid syllabusId, bool isPublished, CancellationToken cancellationToken)
    {
        Syllabus syllabus = await LoadSyllabusAsync(syllabusId, cancellationToken);

        syllabus.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(syllabusId, cancellationToken);
    }

    public async Task DeleteAsync(Guid syllabusId, CancellationToken cancellationToken)
    {
        // โหลดเฉพาะหลักสูตร ไม่รวมรายการลูก ไม่งั้น soft delete จะชนกับแถวที่ EF ติดตามอยู่
        Syllabus syllabus = await LoadSyllabusAsync(syllabusId, cancellationToken);

        _dbContext.Syllabuses.Remove(syllabus);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<SyllabusDetailDto> SelectDetail(IQueryable<Syllabus> query, CancellationToken cancellationToken)
    {
        SyllabusDetailDto? detail = await query
            .Select(syllabus => new SyllabusDetailDto
            {
                Id = syllabus.Id,
                NameTH = syllabus.NameTH,
                NameEN = syllabus.NameEN,
                Year = syllabus.Year,
                DegreeTH = syllabus.DegreeTH,
                DegreeEN = syllabus.DegreeEN,
                TotalCredits = syllabus.TotalCredits,
                PdfFileId = syllabus.PdfFileId,
                IsCurrent = syllabus.IsCurrent,
                Objectives = syllabus.Objectives
                    .OrderBy(objective => objective.SortOrder)
                    .Select(objective => objective.Sentence)
                    .ToList(),
                Occupations = syllabus.Occupations
                    .OrderBy(occupation => occupation.SortOrder)
                    .Select(occupation => occupation.Sentence)
                    .ToList(),
                Subjects = syllabus.Subjects
                    .OrderBy(subject => subject.SubjectCategory)
                    .ThenBy(subject => subject.Code)
                    .ThenBy(subject => subject.Name)
                    .Select(subject => new SubjectDto
                    {
                        Id = subject.Id,
                        Code = subject.Code,
                        Name = subject.Name,
                        Credit = subject.Credit,
                        SubjectCategory = subject.SubjectCategory
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail == null)
        {
            throw new NotFoundException("Syllabus not found.");
        }

        return detail;
    }

    private async Task<Syllabus> LoadSyllabusAsync(Guid syllabusId, CancellationToken cancellationToken)
    {
        Syllabus? syllabus = await _dbContext.Syllabuses
            .FirstOrDefaultAsync(item => item.Id == syllabusId, cancellationToken);
        if (syllabus == null)
        {
            throw new NotFoundException("Syllabus not found.");
        }

        return syllabus;
    }

    private async Task ValidateRequestAsync(SaveSyllabusRequest request, CancellationToken cancellationToken)
    {
        if (request.PdfFileId.HasValue)
        {
            bool fileExists = await _dbContext.Files.AnyAsync(file => file.Id == request.PdfFileId.Value, cancellationToken);
            if (!fileExists)
            {
                throw new BadRequestException("PDF file does not exist.");
            }
        }

        ValidateSentences(request.Objectives, "Objective");
        ValidateSentences(request.Occupations, "Occupation");
    }

    private static void ValidateSentences(List<string>? sentences, string label)
    {
        if (sentences == null)
        {
            return;
        }

        foreach (string sentence in sentences)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                throw new BadRequestException(label + " must not be empty.");
            }

            if (sentence.Trim().Length > 500)
            {
                throw new BadRequestException(label + " must not be longer than 500 characters.");
            }
        }
    }

    private static void CopyRequestToSyllabus(SaveSyllabusRequest request, Syllabus syllabus)
    {
        syllabus.NameTH = request.NameTH.Trim();
        syllabus.NameEN = request.NameEN?.Trim();
        syllabus.Year = request.Year;
        syllabus.DegreeTH = request.DegreeTH?.Trim();
        syllabus.DegreeEN = request.DegreeEN?.Trim();
        syllabus.TotalCredits = request.TotalCredits;
        syllabus.PdfFileId = request.PdfFileId;
        syllabus.IsCurrent = request.IsCurrent;
        syllabus.IsPublished = request.IsPublished;
    }

    private void AddChildren(Syllabus syllabus, SaveSyllabusRequest request)
    {
        if (request.Objectives != null)
        {
            for (int index = 0; index < request.Objectives.Count; index++)
            {
                SyllabusObjective objective = new SyllabusObjective();
                objective.SyllabusId = syllabus.Id;
                objective.Sentence = request.Objectives[index].Trim();
                objective.SortOrder = index;
                _dbContext.SyllabusObjectives.Add(objective);
            }
        }

        if (request.Occupations != null)
        {
            for (int index = 0; index < request.Occupations.Count; index++)
            {
                SyllabusOccupation occupation = new SyllabusOccupation();
                occupation.SyllabusId = syllabus.Id;
                occupation.Sentence = request.Occupations[index].Trim();
                occupation.SortOrder = index;
                _dbContext.SyllabusOccupations.Add(occupation);
            }
        }

        if (request.Subjects != null)
        {
            foreach (SubjectInput input in request.Subjects)
            {
                Subject subject = new Subject();
                subject.SyllabusId = syllabus.Id;
                subject.Code = input.Code?.Trim();
                subject.Name = input.Name.Trim();
                subject.Credit = input.Credit;
                subject.SubjectCategory = input.SubjectCategory;
                _dbContext.Subjects.Add(subject);
            }
        }
    }

    // มีหลักสูตรปัจจุบันได้อันเดียว: ถ้าอันนี้ถูกตั้งเป็นปัจจุบัน ให้ปลดอันอื่นออก
    private async Task ClearCurrentOnOthersAsync(Syllabus syllabus, CancellationToken cancellationToken)
    {
        if (!syllabus.IsCurrent)
        {
            return;
        }

        List<Syllabus> others = await _dbContext.Syllabuses
            .Where(item => item.IsCurrent && item.Id != syllabus.Id)
            .ToListAsync(cancellationToken);

        foreach (Syllabus other in others)
        {
            other.IsCurrent = false;
        }
    }
}
