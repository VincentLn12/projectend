using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.CertificateTemplates;

public class SaveCertificateTemplateRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string SignerName { get; set; } = string.Empty;
    [Required]
    [StringLength(300)]
    public string SignerPosition { get; set; } = string.Empty;
    [StringLength(200)]
    public string? SecondSignerName { get; set; }
    [StringLength(300)]
    public string? SecondSignerPosition { get; set; }
    public Guid? BackgroundFileId { get; set; }
    public Guid? SignatureFileId { get; set; }
    public Guid? SecondSignatureFileId { get; set; }
}
