namespace ComSci.Application.Constants;

public static class FileRules
{
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    public const long MaxRequestBytes = MaxUploadBytes + 512 * 1024;
}
