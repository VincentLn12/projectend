using ComSci.Application.Dtos.CertificateTemplates;

namespace ComSci.Application.Abstractions;

public interface ICertificateTemplateService
{
    Task<List<CertificateTemplateDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<CertificateTemplateDto> GetByIdAsync(Guid templateId, CancellationToken cancellationToken);

    Task<CertificateTemplateDto> CreateAsync(SaveCertificateTemplateRequest request, CancellationToken cancellationToken);

    Task<CertificateTemplateDto> UpdateAsync(Guid templateId, SaveCertificateTemplateRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid templateId, CancellationToken cancellationToken);

    Task<byte[]> CreatePreviewAsync(SaveCertificateTemplateRequest request, CancellationToken cancellationToken);
}
