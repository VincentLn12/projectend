namespace ComSci.Application.Dtos.CertificateTemplates;

public class CertificateLayoutDto
{
    public string PageSize { get; set; } = "A4";
    public bool Landscape { get; set; } = true;
    public Guid? BackgroundFileId { get; set; }
    public bool ShowFrameWithoutBackground { get; set; }
    public List<CertificateElementDto> Elements { get; set; } = new List<CertificateElementDto>();
}
