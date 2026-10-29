using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Projects;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _dbContext;

    public ProjectService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ProjectListItemDto>> GetPublishedAsync(ProjectQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Project> projectQuery = _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.IsPublished);
        projectQuery = ApplyFilters(projectQuery, query, true);

        int total = await projectQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<ProjectListItemDto> items = await projectQuery
            .OrderByDescending(project => project.AcademicYear)
            .ThenByDescending(project => project.CreatedAt)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(project => new ProjectListItemDto
            {
                Id = project.Id,
                NameTH = project.NameTH,
                NameEN = project.NameEN,
                AcademicYear = project.AcademicYear,
                KeyWords = project.KeyWords,
                ImageFileId = project.ImageFileId,
                StudentName = project.Student.ShowProfile ? project.Student.FullName : PrivacyText.AnonymousName
            })
            .ToListAsync(cancellationToken);

        PagedResult<ProjectListItemDto> result = new PagedResult<ProjectListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<ProjectDetailDto> GetPublishedByIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        ProjectDetailDto? detail = await _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.Id == projectId && project.IsPublished)
            .Select(project => new ProjectDetailDto
            {
                Id = project.Id,
                NameTH = project.NameTH,
                NameEN = project.NameEN,
                Description = project.Description,
                AcademicYear = project.AcademicYear,
                KeyWords = project.KeyWords,
                VideoUrl = project.VideoUrl,
                WebUrl = project.WebUrl,
                GithubUrl = project.GithubUrl,
                ImageFileId = project.ImageFileId,
                PdfFileId = project.PdfFileId,
                StudentName = project.Student.ShowProfile ? project.Student.FullName : PrivacyText.AnonymousName,
                Advisors = project.Advisors
                    .OrderBy(advisor => advisor.Role)
                    .Select(advisor => new ProjectAdvisorDto
                    {
                        LecturerId = advisor.LecturerId,
                        LecturerName = advisor.Lecturer.FullName,
                        Role = advisor.Role
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail == null)
        {
            throw new NotFoundException("Project not found.");
        }

        return detail;
    }

    public async Task<PagedResult<AdminProjectListItemDto>> GetAllAsync(AdminProjectQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Project> projectQuery = _dbContext.Projects.AsNoTracking();
        projectQuery = ApplyFilters(projectQuery, query, false);

        if (query.IsPublished.HasValue)
        {
            bool wantedState = query.IsPublished.Value;
            projectQuery = projectQuery.Where(project => project.IsPublished == wantedState);
        }

        int total = await projectQuery.CountAsync(cancellationToken);

        int itemsToSkip = (query.Page - 1) * query.PageSize;
        List<AdminProjectListItemDto> items = await projectQuery
            .OrderByDescending(project => project.CreatedAt)
            .Skip(itemsToSkip)
            .Take(query.PageSize)
            .Select(project => new AdminProjectListItemDto
            {
                Id = project.Id,
                NameTH = project.NameTH,
                AcademicYear = project.AcademicYear,
                StudentName = project.Student.FullName,
                IsPublished = project.IsPublished,
                CreatedAt = project.CreatedAt
            })
            .ToListAsync(cancellationToken);

        PagedResult<AdminProjectListItemDto> result = new PagedResult<AdminProjectListItemDto>();
        result.Items = items;
        result.Page = query.Page;
        result.PageSize = query.PageSize;
        result.Total = total;
        return result;
    }

    public async Task<AdminProjectDto> GetByIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        AdminProjectDto? dto = await _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.Id == projectId)
            .Select(project => new AdminProjectDto
            {
                Id = project.Id,
                NameTH = project.NameTH,
                NameEN = project.NameEN,
                Description = project.Description,
                AcademicYear = project.AcademicYear,
                KeyWords = project.KeyWords,
                VideoUrl = project.VideoUrl,
                WebUrl = project.WebUrl,
                GithubUrl = project.GithubUrl,
                ImageFileId = project.ImageFileId,
                PdfFileId = project.PdfFileId,
                StudentId = project.StudentId,
                StudentName = project.Student.FullName,
                IsPublished = project.IsPublished,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Advisors = project.Advisors
                    .OrderBy(advisor => advisor.Role)
                    .Select(advisor => new ProjectAdvisorDto
                    {
                        LecturerId = advisor.LecturerId,
                        LecturerName = advisor.Lecturer.FullName,
                        Role = advisor.Role
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw new NotFoundException("Project not found.");
        }

        return dto;
    }

    public async Task<AdminProjectDto> CreateAsync(SaveProjectRequest request, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);

        Project project = new Project();
        CopyRequestToProject(request, project);
        ReplaceAdvisors(project, request);

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(project.Id, cancellationToken);
    }

    public async Task<AdminProjectDto> UpdateAsync(Guid projectId, SaveProjectRequest request, CancellationToken cancellationToken)
    {
        Project? project = await _dbContext.Projects
            .Include(item => item.Advisors)
            .FirstOrDefaultAsync(item => item.Id == projectId, cancellationToken);
        if (project == null)
        {
            throw new NotFoundException("Project not found.");
        }

        await ValidateRequestAsync(request, cancellationToken);

        CopyRequestToProject(request, project);
        ReplaceAdvisors(project, request);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(projectId, cancellationToken);
    }

    public async Task<AdminProjectDto> SetPublishedAsync(Guid projectId, bool isPublished, CancellationToken cancellationToken)
    {
        Project project = await LoadProjectAsync(projectId, cancellationToken);

        project.IsPublished = isPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(projectId, cancellationToken);
    }

    public async Task DeleteAsync(Guid projectId, CancellationToken cancellationToken)
    {
        // โหลดเฉพาะโครงงาน ไม่รวมที่ปรึกษา ไม่งั้น soft delete จะชนกับแถวที่ EF ติดตามอยู่
        Project project = await LoadProjectAsync(projectId, cancellationToken);

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<short>> GetAcademicYearsAsync(CancellationToken cancellationToken)
    {
        List<short> years = await _dbContext.Projects
            .AsNoTracking()
            .Select(project => project.AcademicYear)
            .Distinct()
            .OrderByDescending(year => year)
            .ToListAsync(cancellationToken);

        return years;
    }

    private static IQueryable<Project> ApplyFilters(IQueryable<Project> projectQuery, ProjectQuery query, bool isPublicView)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string keyword = query.Search.Trim();

            // หน้าสาธารณะค้นหาจากชื่อนักศึกษาได้เฉพาะคนที่ยินยอมให้แสดงชื่อ ไม่งั้นจะเดาชื่อคนที่ซ่อนอยู่ได้
            projectQuery = projectQuery.Where(project =>
                project.NameTH.Contains(keyword)
                || (project.NameEN != null && project.NameEN.Contains(keyword))
                || ((!isPublicView || project.Student.ShowProfile) && project.Student.FullName.Contains(keyword)));
        }

        if (query.AcademicYear.HasValue)
        {
            short year = query.AcademicYear.Value;
            projectQuery = projectQuery.Where(project => project.AcademicYear == year);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            string tag = query.Keyword.Trim();
            projectQuery = projectQuery.Where(project => project.KeyWords != null && project.KeyWords.Contains(tag));
        }

        return projectQuery;
    }

    private async Task<Project> LoadProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        Project? project = await _dbContext.Projects
            .FirstOrDefaultAsync(item => item.Id == projectId, cancellationToken);
        if (project == null)
        {
            throw new NotFoundException("Project not found.");
        }

        return project;
    }

    private async Task ValidateRequestAsync(SaveProjectRequest request, CancellationToken cancellationToken)
    {
        bool studentExists = await _dbContext.Users.AnyAsync(user => user.Id == request.StudentId, cancellationToken);
        if (!studentExists)
        {
            throw new BadRequestException("Student does not exist.");
        }

        await EnsureFileExistsAsync(request.ImageFileId, "Image", cancellationToken);
        await EnsureFileExistsAsync(request.PdfFileId, "PDF", cancellationToken);

        List<Guid> lecturerIds = new List<Guid>();
        if (request.Advisors != null)
        {
            foreach (ProjectAdvisorInput advisor in request.Advisors)
            {
                lecturerIds.Add(advisor.LecturerId);
            }
        }

        if (lecturerIds.Distinct().Count() != lecturerIds.Count)
        {
            throw new BadRequestException("The same lecturer is listed more than once.");
        }

        if (lecturerIds.Count > 0)
        {
            int existingCount = await _dbContext.Lecturers.CountAsync(lecturer => lecturerIds.Contains(lecturer.Id), cancellationToken);
            if (existingCount != lecturerIds.Count)
            {
                throw new BadRequestException("Some lecturers do not exist.");
            }
        }
    }

    private async Task EnsureFileExistsAsync(Guid? fileId, string label, CancellationToken cancellationToken)
    {
        if (!fileId.HasValue)
        {
            return;
        }

        bool exists = await _dbContext.Files.AnyAsync(file => file.Id == fileId.Value, cancellationToken);
        if (!exists)
        {
            throw new BadRequestException(label + " file does not exist.");
        }
    }

    private static void CopyRequestToProject(SaveProjectRequest request, Project project)
    {
        project.NameTH = request.NameTH.Trim();
        project.NameEN = request.NameEN?.Trim();
        project.Description = request.Description;
        project.AcademicYear = request.AcademicYear;
        project.KeyWords = request.KeyWords?.Trim();
        project.VideoUrl = request.VideoUrl?.Trim();
        project.WebUrl = request.WebUrl?.Trim();
        project.GithubUrl = request.GithubUrl?.Trim();
        project.ImageFileId = request.ImageFileId;
        project.PdfFileId = request.PdfFileId;
        project.StudentId = request.StudentId;
        project.IsPublished = request.IsPublished;
    }

    private void ReplaceAdvisors(Project project, SaveProjectRequest request)
    {
        // ProjectAdvisor มี GUID ตั้งแต่สร้าง ต้อง Add ที่ DbSet ตรงๆ (เหมือน NewsFile) ไม่ใช่ project.Advisors.Add
        List<ProjectAdvisor> oldAdvisors = project.Advisors.ToList();
        _dbContext.ProjectAdvisors.RemoveRange(oldAdvisors);

        if (request.Advisors == null)
        {
            return;
        }

        foreach (ProjectAdvisorInput input in request.Advisors)
        {
            ProjectAdvisor advisor = new ProjectAdvisor();
            advisor.ProjectId = project.Id;
            advisor.LecturerId = input.LecturerId;
            advisor.Role = input.Role;
            _dbContext.ProjectAdvisors.Add(advisor);
        }
    }
}
