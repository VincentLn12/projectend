using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.CertificateTemplates;
using ComSci.Application.Exceptions;
using ComSci.Application.Helpers;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class CertificateTemplateService : ICertificateTemplateService
{
    private readonly AppDbContext _dbContext;
    private readonly CertificateTemplateRenderer _renderer;

    public CertificateTemplateService(AppDbContext dbContext, CertificateTemplateRenderer renderer)
    {
        _dbContext = dbContext;
        _renderer = renderer;
    }

    public async Task<List<CertificateTemplateDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<CertificateTemplate> templates = await _dbContext.CertificateTemplates
            .AsNoTracking()
            .OrderBy(template => template.Name)
            .ToListAsync(cancellationToken);

        return templates.Select(MapToDto).ToList();
    }

    public async Task<CertificateTemplateDto> GetByIdAsync(Guid templateId, CancellationToken cancellationToken)
    {
        CertificateTemplate template = await LoadAsync(templateId, cancellationToken);
        return MapToDto(template);
    }

    public async Task<CertificateTemplateDto> CreateAsync(SaveCertificateTemplateRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, cancellationToken);

        CertificateTemplate template = new CertificateTemplate();
        CopyRequestToTemplate(request, template);

        _dbContext.CertificateTemplates.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(template);
    }

    public async Task<CertificateTemplateDto> UpdateAsync(Guid templateId, SaveCertificateTemplateRequest request, CancellationToken cancellationToken)
    {
        CertificateTemplate template = await LoadAsync(templateId, cancellationToken);
        await ValidateAsync(request, cancellationToken);

        CopyRequestToTemplate(request, template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(template);
    }

    public async Task DeleteAsync(Guid templateId, CancellationToken cancellationToken)
    {
        CertificateTemplate template = await LoadAsync(templateId, cancellationToken);

        bool isUsed = await _dbContext.ShortCourses.AnyAsync(course => course.CertificateTemplateId == templateId, cancellationToken);
        if (isUsed)
        {
            throw new ConflictException("ลบไม่ได้เพราะมีหลักสูตรใช้แม่แบบนี้อยู่");
        }

        _dbContext.CertificateTemplates.Remove(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<byte[]> CreatePreviewAsync(SaveCertificateTemplateRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, cancellationToken);

        CertificateLayoutDto layout = new CertificateLayoutDto();
        layout.PageSize = request.PageSize;
        layout.Landscape = request.Landscape;
        layout.BackgroundFileId = request.BackgroundFileId;
        layout.Elements = request.Elements;

        List<IReadOnlyDictionary<string, string>> variableSets = new List<IReadOnlyDictionary<string, string>>();
        variableSets.Add(CertificateVariables.Sample());
        return await _renderer.RenderAsync(layout, variableSets, cancellationToken);
    }

    private async Task<CertificateTemplate> LoadAsync(Guid templateId, CancellationToken cancellationToken)
    {
        CertificateTemplate? template = await _dbContext.CertificateTemplates
            .FirstOrDefaultAsync(item => item.Id == templateId, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException("ไม่พบแม่แบบเกียรติบัตร");
        }

        return template;
    }

    private async Task ValidateAsync(SaveCertificateTemplateRequest request, CancellationToken cancellationToken)
    {
        await EnsureImageFileAsync(request.BackgroundFileId, "ภาพพื้นหลัง", cancellationToken);

        foreach (CertificateElementDto element in request.Elements)
        {
            if (element.Type == "image")
            {
                if (!element.ImageFileId.HasValue)
                {
                    throw new BadRequestException("องค์ประกอบรูปภาพต้องเลือกไฟล์รูป");
                }

                await EnsureImageFileAsync(element.ImageFileId, "รูปภาพในแม่แบบ", cancellationToken);
            }
        }
    }

    private async Task EnsureImageFileAsync(Guid? fileId, string label, CancellationToken cancellationToken)
    {
        if (!fileId.HasValue)
        {
            return;
        }

        FileRecord? file = await _dbContext.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == fileId.Value, cancellationToken);
        if (file == null)
        {
            throw new BadRequestException("ไม่พบไฟล์" + label);
        }

        if (!file.ContentType.StartsWith("image/"))
        {
            throw new BadRequestException(label + "ต้องเป็นไฟล์รูปภาพ");
        }
    }

    private static void CopyRequestToTemplate(SaveCertificateTemplateRequest request, CertificateTemplate template)
    {
        template.Name = request.Name.Trim();
        template.PageSize = request.PageSize;
        template.Landscape = request.Landscape;
        template.BackgroundFileId = request.BackgroundFileId;
        template.ElementsJson = CertificateLayoutDefaults.SerializeElements(request.Elements);
    }

    private static CertificateTemplateDto MapToDto(CertificateTemplate template)
    {
        CertificateTemplateDto dto = new CertificateTemplateDto();
        dto.Id = template.Id;
        dto.Name = template.Name;
        dto.PageSize = template.PageSize;
        dto.Landscape = template.Landscape;
        dto.BackgroundFileId = template.BackgroundFileId;
        dto.Elements = CertificateLayoutDefaults.ElementsOf(template);
        return dto;
    }
}
