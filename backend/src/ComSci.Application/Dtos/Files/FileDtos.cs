namespace ComSci.Application.Dtos.Files;

public record FileDto(Guid Id, string OriginalName, string ContentType, long SizeBytes, string Url, DateTime UploadedAt);

/// <summary>A stored file opened for reading. The caller owns (and disposes) the stream.</summary>
public record FileContent(Stream Content, string ContentType);
