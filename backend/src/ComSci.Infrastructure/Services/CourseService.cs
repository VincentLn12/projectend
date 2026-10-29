using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Courses;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _dbContext;

    public CourseService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CourseListItemDto>> GetPublishedAsync(PageQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Course> courseQuery = _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsPublished);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string keyword = query.Search.Trim();
            courseQuery = courseQuery.Where(course => course.Title.Contains(keyword));
        }

        int total = await courseQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<CourseListItemDto> items = await courseQuery
            .OrderByDescending(course => course.CreatedAt)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(course => new CourseListItemDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                LecturerName = course.Lecturer != null ? course.Lecturer.FullName : null,
                MainImageFileId = course.Photos
                    .OrderByDescending(photo => photo.IsMain)
                    .Select(photo => (Guid?)photo.FileId)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        PagedResult<CourseListItemDto> result = new PagedResult<CourseListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<CourseDetailDto> GetPublishedByIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        CourseDetailDto? detail = await _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Id == courseId && course.IsPublished)
            .Select(course => new CourseDetailDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                LecturerName = course.Lecturer != null ? course.Lecturer.FullName : null,
                ImageFileIds = course.Photos
                    .OrderByDescending(photo => photo.IsMain)
                    .Select(photo => photo.FileId)
                    .ToList(),
                Generations = course.Generations
                    .OrderBy(generation => generation.StartDate)
                    .Select(generation => new GenerationDto
                    {
                        Id = generation.Id,
                        CourseId = generation.CourseId,
                        SubTitle = generation.SubTitle,
                        Description = generation.Description,
                        StartDate = generation.StartDate,
                        EndDate = generation.EndDate,
                        Quantity = generation.Quantity,
                        RegisteredCount = generation.Registrations.Count(registration => registration.Status != RegistrationStatus.Cancelled),
                        Fee = generation.Fee,
                        PhotoFileId = generation.PhotoFileId,
                        IsCancelled = generation.IsCancelled
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail == null)
        {
            throw new NotFoundException("Course not found.");
        }

        MarkRegistrationOpen(detail.Generations);
        return detail;
    }

    public async Task<PagedResult<AdminCourseListItemDto>> GetAllAsync(AdminCourseQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Course> courseQuery = _dbContext.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string keyword = query.Search.Trim();
            courseQuery = courseQuery.Where(course => course.Title.Contains(keyword));
        }

        if (query.IsPublished.HasValue)
        {
            bool wantedState = query.IsPublished.Value;
            courseQuery = courseQuery.Where(course => course.IsPublished == wantedState);
        }

        int total = await courseQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<AdminCourseListItemDto> items = await courseQuery
            .OrderByDescending(course => course.CreatedAt)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(course => new AdminCourseListItemDto
            {
                Id = course.Id,
                Title = course.Title,
                LecturerName = course.Lecturer != null ? course.Lecturer.FullName : null,
                IsPublished = course.IsPublished,
                GenerationCount = course.Generations.Count,
                CreatedAt = course.CreatedAt
            })
            .ToListAsync(cancellationToken);

        PagedResult<AdminCourseListItemDto> result = new PagedResult<AdminCourseListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<AdminCourseDto> GetByIdAsync(Guid courseId, CancellationToken cancellationToken)
    {
        AdminCourseDto? dto = await _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Id == courseId)
            .Select(course => new AdminCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                LecturerId = course.LecturerId,
                LecturerName = course.Lecturer != null ? course.Lecturer.FullName : null,
                IsPublished = course.IsPublished,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                Photos = course.Photos
                    .OrderByDescending(photo => photo.IsMain)
                    .Select(photo => new CoursePhotoDto
                    {
                        FileId = photo.FileId,
                        IsMain = photo.IsMain
                    })
                    .ToList(),
                Generations = course.Generations
                    .OrderBy(generation => generation.StartDate)
                    .Select(generation => new GenerationDto
                    {
                        Id = generation.Id,
                        CourseId = generation.CourseId,
                        SubTitle = generation.SubTitle,
                        Description = generation.Description,
                        StartDate = generation.StartDate,
                        EndDate = generation.EndDate,
                        Quantity = generation.Quantity,
                        RegisteredCount = generation.Registrations.Count(registration => registration.Status != RegistrationStatus.Cancelled),
                        Fee = generation.Fee,
                        PhotoFileId = generation.PhotoFileId,
                        IsCancelled = generation.IsCancelled
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw new NotFoundException("Course not found.");
        }

        MarkRegistrationOpen(dto.Generations);
        return dto;
    }

    public async Task<AdminCourseDto> CreateAsync(SaveCourseRequest request, CancellationToken cancellationToken)
    {
        await ValidateCourseRequestAsync(request, cancellationToken);

        Course course = new Course();
        CopyRequestToCourse(request, course);
        AddPhotos(course, request);

        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(course.Id, cancellationToken);
    }

    public async Task<AdminCourseDto> UpdateAsync(Guid courseId, SaveCourseRequest request, CancellationToken cancellationToken)
    {
        Course? course = await _dbContext.Courses
            .Include(item => item.Photos)
            .FirstOrDefaultAsync(item => item.Id == courseId, cancellationToken);
        if (course == null)
        {
            throw new NotFoundException("Course not found.");
        }

        await ValidateCourseRequestAsync(request, cancellationToken);

        CopyRequestToCourse(request, course);

        // ห้าม course.Photos.Clear() และต้อง Add ที่ DbSet ตรงๆ (เหตุผลเดียวกับรูปของข่าว)
        _dbContext.CoursePhotos.RemoveRange(course.Photos.ToList());
        AddPhotos(course, request);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(courseId, cancellationToken);
    }

    public async Task<AdminCourseDto> SetPublishedAsync(Guid courseId, bool isPublished, CancellationToken cancellationToken)
    {
        Course course = await LoadCourseAsync(courseId, cancellationToken);

        course.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(courseId, cancellationToken);
    }

    public async Task DeleteAsync(Guid courseId, CancellationToken cancellationToken)
    {
        // โหลดเฉพาะหลักสูตร ไม่รวมรูป ไม่งั้น soft delete จะชนกับแถวที่ EF ติดตามอยู่
        Course course = await LoadCourseAsync(courseId, cancellationToken);

        bool hasActiveRegistrations = await _dbContext.CourseRegistrations.AnyAsync(
            registration => registration.Generation.CourseId == courseId
                && registration.Status != RegistrationStatus.Cancelled,
            cancellationToken);
        if (hasActiveRegistrations)
        {
            throw new ConflictException("Course still has registrations. Cancel them first.");
        }

        _dbContext.Courses.Remove(course);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<GenerationDto> CreateGenerationAsync(Guid courseId, SaveGenerationRequest request, CancellationToken cancellationToken)
    {
        await LoadCourseAsync(courseId, cancellationToken);
        await ValidateGenerationRequestAsync(request, 0, cancellationToken);

        Generation generation = new Generation();
        generation.CourseId = courseId;
        CopyRequestToGeneration(request, generation);

        _dbContext.Generations.Add(generation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetGenerationAsync(generation.Id, cancellationToken);
    }

    public async Task<GenerationDto> UpdateGenerationAsync(Guid generationId, SaveGenerationRequest request, CancellationToken cancellationToken)
    {
        Generation generation = await LoadGenerationAsync(generationId, cancellationToken);

        int registeredCount = await CountActiveRegistrationsAsync(generationId, cancellationToken);
        await ValidateGenerationRequestAsync(request, registeredCount, cancellationToken);

        CopyRequestToGeneration(request, generation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetGenerationAsync(generationId, cancellationToken);
    }

    public async Task DeleteGenerationAsync(Guid generationId, CancellationToken cancellationToken)
    {
        Generation generation = await LoadGenerationAsync(generationId, cancellationToken);

        int registeredCount = await CountActiveRegistrationsAsync(generationId, cancellationToken);
        if (registeredCount > 0)
        {
            throw new ConflictException("Generation has registrations. Mark it as cancelled instead of deleting it.");
        }

        _dbContext.Generations.Remove(generation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<GenerationDto> GetGenerationAsync(Guid generationId, CancellationToken cancellationToken)
    {
        GenerationDto? dto = await _dbContext.Generations
            .AsNoTracking()
            .Where(generation => generation.Id == generationId)
            .Select(generation => new GenerationDto
            {
                Id = generation.Id,
                CourseId = generation.CourseId,
                SubTitle = generation.SubTitle,
                Description = generation.Description,
                StartDate = generation.StartDate,
                EndDate = generation.EndDate,
                Quantity = generation.Quantity,
                RegisteredCount = generation.Registrations.Count(registration => registration.Status != RegistrationStatus.Cancelled),
                Fee = generation.Fee,
                PhotoFileId = generation.PhotoFileId,
                IsCancelled = generation.IsCancelled
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw new NotFoundException("Generation not found.");
        }

        MarkRegistrationOpen(new List<GenerationDto> { dto });
        return dto;
    }

    private static void MarkRegistrationOpen(List<GenerationDto> generations)
    {
        DateTime now = DateTime.UtcNow;

        foreach (GenerationDto generation in generations)
        {
            generation.IsRegistrationOpen = !generation.IsCancelled
                && generation.StartDate > now
                && generation.RegisteredCount < generation.Quantity;
        }
    }

    private async Task<Course> LoadCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        Course? course = await _dbContext.Courses
            .FirstOrDefaultAsync(item => item.Id == courseId, cancellationToken);
        if (course == null)
        {
            throw new NotFoundException("Course not found.");
        }

        return course;
    }

    private async Task<Generation> LoadGenerationAsync(Guid generationId, CancellationToken cancellationToken)
    {
        Generation? generation = await _dbContext.Generations
            .FirstOrDefaultAsync(item => item.Id == generationId, cancellationToken);
        if (generation == null)
        {
            throw new NotFoundException("Generation not found.");
        }

        return generation;
    }

    private async Task<int> CountActiveRegistrationsAsync(Guid generationId, CancellationToken cancellationToken)
    {
        return await _dbContext.CourseRegistrations.CountAsync(
            registration => registration.GenerationId == generationId
                && registration.Status != RegistrationStatus.Cancelled,
            cancellationToken);
    }

    private async Task ValidateCourseRequestAsync(SaveCourseRequest request, CancellationToken cancellationToken)
    {
        if (request.LecturerId.HasValue)
        {
            bool lecturerExists = await _dbContext.Lecturers.AnyAsync(lecturer => lecturer.Id == request.LecturerId.Value, cancellationToken);
            if (!lecturerExists)
            {
                throw new BadRequestException("Lecturer does not exist.");
            }
        }

        List<Guid> fileIds = GetDistinctFileIds(request);

        if (request.MainFileId.HasValue && !fileIds.Contains(request.MainFileId.Value))
        {
            throw new BadRequestException("MainFileId must be one of FileIds.");
        }

        if (fileIds.Count > 0)
        {
            int existingCount = await _dbContext.Files.CountAsync(file => fileIds.Contains(file.Id), cancellationToken);
            if (existingCount != fileIds.Count)
            {
                throw new BadRequestException("Some files do not exist.");
            }
        }
    }

    private async Task ValidateGenerationRequestAsync(SaveGenerationRequest request, int registeredCount, CancellationToken cancellationToken)
    {
        if (request.EndDate < request.StartDate)
        {
            throw new BadRequestException("EndDate must not be before StartDate.");
        }

        if (request.Quantity < registeredCount)
        {
            throw new BadRequestException("Quantity cannot be lower than the number of current registrations (" + registeredCount + ").");
        }

        if (request.PhotoFileId.HasValue)
        {
            bool fileExists = await _dbContext.Files.AnyAsync(file => file.Id == request.PhotoFileId.Value, cancellationToken);
            if (!fileExists)
            {
                throw new BadRequestException("Photo file does not exist.");
            }
        }
    }

    private static List<Guid> GetDistinctFileIds(SaveCourseRequest request)
    {
        if (request.FileIds == null)
        {
            return new List<Guid>();
        }

        return request.FileIds.Distinct().ToList();
    }

    private static void CopyRequestToCourse(SaveCourseRequest request, Course course)
    {
        course.Title = request.Title.Trim();
        course.Description = request.Description;
        course.LecturerId = request.LecturerId;
        course.IsPublished = request.IsPublished;
    }

    private void AddPhotos(Course course, SaveCourseRequest request)
    {
        List<Guid> fileIds = GetDistinctFileIds(request);

        Guid mainFileId = Guid.Empty;
        if (request.MainFileId.HasValue)
        {
            mainFileId = request.MainFileId.Value;
        }
        else if (fileIds.Count > 0)
        {
            mainFileId = fileIds[0];
        }

        foreach (Guid fileId in fileIds)
        {
            CoursePhoto photo = new CoursePhoto();
            photo.CourseId = course.Id;
            photo.FileId = fileId;
            photo.IsMain = fileId == mainFileId;
            _dbContext.CoursePhotos.Add(photo);
        }
    }

    private static void CopyRequestToGeneration(SaveGenerationRequest request, Generation generation)
    {
        generation.SubTitle = request.SubTitle?.Trim();
        generation.Description = request.Description;
        generation.StartDate = request.StartDate;
        generation.EndDate = request.EndDate;
        generation.Quantity = request.Quantity;
        generation.Fee = request.Fee;
        generation.PhotoFileId = request.PhotoFileId;
        generation.IsCancelled = request.IsCancelled;
    }
}
