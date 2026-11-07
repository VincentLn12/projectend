using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.CertificateTemplates;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class CertificateTemplateService : ICertificateTemplateService
{
    private readonly AppDbContext _dbContext;

    public CertificateTemplateService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
        if (request.BackgroundFileId.HasValue)
        {
            FileRecord? file = await _dbContext.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == request.BackgroundFileId.Value, cancellationToken);
            if (file == null)
            {
                throw new BadRequestException("ไม่พบไฟล์ภาพพื้นหลัง");
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                throw new BadRequestException("ภาพพื้นหลังต้องเป็นไฟล์รูปภาพ");
            }
        }
    }

    private static void CopyRequestToTemplate(SaveCertificateTemplateRequest request, CertificateTemplate template)
    {
        template.Name = request.Name.Trim();
        template.SignerName = request.SignerName.Trim();
        template.SignerPosition = request.SignerPosition.Trim();
        template.SecondSignerName = string.IsNullOrWhiteSpace(request.SecondSignerName) ? null : request.SecondSignerName.Trim();
        template.SecondSignerPosition = string.IsNullOrWhiteSpace(request.SecondSignerPosition) ? null : request.SecondSignerPosition.Trim();
        template.BackgroundFileId = request.BackgroundFileId;
    }

    private static CertificateTemplateDto MapToDto(CertificateTemplate template)
    {
        CertificateTemplateDto dto = new CertificateTemplateDto();
        dto.Id = template.Id;
        dto.Name = template.Name;
        dto.SignerName = template.SignerName;
        dto.SignerPosition = template.SignerPosition;
        dto.SecondSignerName = template.SecondSignerName;
        dto.SecondSignerPosition = template.SecondSignerPosition;
        dto.BackgroundFileId = template.BackgroundFileId;
        return dto;
    }
}
