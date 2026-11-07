namespace ComSci.Application.Dtos.CertificateTemplates;

public class CertificateTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SignerName { get; set; } = string.Empty;
    public string SignerPosition { get; set; } = string.Empty;
    public string? SecondSignerName { get; set; }
    public string? SecondSignerPosition { get; set; }
    public Guid? BackgroundFileId { get; set; }
}
