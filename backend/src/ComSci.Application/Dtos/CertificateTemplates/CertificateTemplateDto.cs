namespace ComSci.Application.Dtos.CertificateTemplates;

public class CertificateTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PageSize { get; set; } = "A4";
    public bool Landscape { get; set; } = true;
    public Guid? BackgroundFileId { get; set; }
    public List<CertificateElementDto> Elements { get; set; } = new List<CertificateElementDto>();
}
