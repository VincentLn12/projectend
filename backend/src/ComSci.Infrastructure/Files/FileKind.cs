namespace ComSci.Infrastructure.Files;

public class FileKind
{
    public string ContentType { get; set; } = string.Empty;

    public List<string> Extensions { get; set; } = new List<string>();

    public FileKind(string contentType, params string[] extensions)
    {
        ContentType = contentType;
        Extensions = new List<string>(extensions);
    }
}
