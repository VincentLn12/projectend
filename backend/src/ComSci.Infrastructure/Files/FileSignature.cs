namespace ComSci.Infrastructure.Files;

/// <summary>Identifies a file by its leading bytes, so a client-supplied name or Content-Type is never trusted.</summary>
public static class FileSignature
{
    public record Kind(string ContentType, string[] Extensions);

    private static readonly Kind Png = new("image/png", [".png"]);
    private static readonly Kind Jpeg = new("image/jpeg", [".jpg", ".jpeg"]);
    private static readonly Kind Gif = new("image/gif", [".gif"]);
    private static readonly Kind Webp = new("image/webp", [".webp"]);
    private static readonly Kind Pdf = new("application/pdf", [".pdf"]);

    public static Kind? Detect(ReadOnlySpan<byte> h)
    {
        ReadOnlySpan<byte> png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        ReadOnlySpan<byte> jpeg = [0xFF, 0xD8, 0xFF];
        if (h.StartsWith(png)) return Png;
        if (h.StartsWith(jpeg)) return Jpeg;
        if (h.StartsWith("GIF87a"u8) || h.StartsWith("GIF89a"u8)) return Gif;
        if (h.Length >= 12 && h[..4].SequenceEqual("RIFF"u8) && h.Slice(8, 4).SequenceEqual("WEBP"u8)) return Webp;
        if (h.StartsWith("%PDF-"u8)) return Pdf;
        return null;
    }
}
