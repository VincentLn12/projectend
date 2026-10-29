using System.IO;

namespace ComSci.Application.Dtos.Files;

public class FileContent
{
    public Stream Content { get; set; } = Stream.Null;
    public string ContentType { get; set; } = string.Empty;
}
