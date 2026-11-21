using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class CertificateTemplate : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string PageSize { get; set; } = "A4";
    public bool Landscape { get; set; } = true;
    public string? ElementsJson { get; set; }
    public string SignerName { get; set; } = string.Empty;
    public string SignerPosition { get; set; } = string.Empty;
    public string? SecondSignerName { get; set; }
    public string? SecondSignerPosition { get; set; }
    public Guid? BackgroundFileId { get; set; }
    public FileRecord? BackgroundFile { get; set; }
    public Guid? SignatureFileId { get; set; }
    public FileRecord? SignatureFile { get; set; }
    public Guid? SecondSignatureFileId { get; set; }
    public FileRecord? SecondSignatureFile { get; set; }
}
