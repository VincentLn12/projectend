using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.CertificateTemplates;

public class CertificateElementDto
{
    [Required]
    [RegularExpression("^(text|image)$")]
    public string Type { get; set; } = "text";

    [Range(0, 100)]
    public double X { get; set; }

    [Range(0, 100)]
    public double Y { get; set; }

    [Range(1, 100)]
    public double Width { get; set; } = 50;

    [Range(1, 100)]
    public double Height { get; set; } = 10;

    [StringLength(1000)]
    public string? Text { get; set; }

    [Range(6, 200)]
    public double FontSize { get; set; } = 24;

    public bool Bold { get; set; }

    public bool Italic { get; set; }

    public bool Underline { get; set; }

    [RegularExpression("^#[0-9a-fA-F]{6}$")]
    public string Color { get; set; } = "#000000";

    [RegularExpression("^(left|center|right)$")]
    public string Align { get; set; } = "center";

    public Guid? ImageFileId { get; set; }
}
