namespace ComSci.Application.Dtos.Files;

public class FileDto
{
    public Guid Id { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
