using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.CertificateTemplates;

public class SaveCertificateTemplateRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(A4|A5|Letter)$")]
    public string PageSize { get; set; } = "A4";

    public bool Landscape { get; set; } = true;

    public Guid? BackgroundFileId { get; set; }

    [MaxLength(80)]
    public List<CertificateElementDto> Elements { get; set; } = new List<CertificateElementDto>();
}
