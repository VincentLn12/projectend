namespace ComSci.Application.Options;

public class StorageOptions
{
    public const string Section = "Storage";

    /// <summary>Folder for uploaded files. A relative value is resolved against the app's content root.</summary>
    public string RootPath { get; set; } = "uploads";
}
