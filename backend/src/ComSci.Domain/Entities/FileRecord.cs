namespace ComSci.Domain.Entities;

public class FileRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}
