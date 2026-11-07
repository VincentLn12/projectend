using ComSci.Domain.Common;

namespace ComSci.Domain.Entities;

public class CertificateTemplate : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string SignerName { get; set; } = string.Empty;
    public string SignerPosition { get; set; } = string.Empty;
    public string? SecondSignerName { get; set; }
    public string? SecondSignerPosition { get; set; }
    public Guid? BackgroundFileId { get; set; }
    public FileRecord? BackgroundFile { get; set; }
}
