namespace ComSci.Application.Constants;

public static class FileRules
{
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    /// <summary>Upload limit plus room for the multipart envelope.</summary>
    public const long MaxRequestBytes = MaxUploadBytes + 512 * 1024;
}
