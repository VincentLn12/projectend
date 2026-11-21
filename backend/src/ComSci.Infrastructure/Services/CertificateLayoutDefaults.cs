using System.Text.Json;
using ComSci.Application.Dtos.CertificateTemplates;
using ComSci.Application.Helpers;
using ComSci.Domain.Entities;

namespace ComSci.Infrastructure.Services;

public static class CertificateLayoutDefaults
{
    private const string IssuerLine = "สาขาวิชาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยราชภัฏกาญจนบุรี";

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public static List<CertificateElementDto> ParseElements(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<CertificateElementDto>();
        }

        return JsonSerializer.Deserialize<List<CertificateElementDto>>(json, JsonOptions) ?? new List<CertificateElementDto>();
    }

    public static string SerializeElements(List<CertificateElementDto> elements)
    {
        return JsonSerializer.Serialize(elements, JsonOptions);
    }

    public static List<CertificateElementDto> ElementsOf(CertificateTemplate template)
    {
        if (template.ElementsJson != null)
        {
            return ParseElements(template.ElementsJson);
        }

        return BuildStandardElements(
            template.SignerName,
            template.SignerPosition,
            template.SignatureFileId,
            template.SecondSignerName,
            template.SecondSignerPosition,
            template.SecondSignatureFileId);
    }

    public static CertificateLayoutDto ResolveLayout(CertificateTemplate? template)
    {
        CertificateLayoutDto layout = new CertificateLayoutDto();
        if (template == null)
        {
            layout.ShowFrameWithoutBackground = true;
            layout.Elements = BuildStandardElements(null, null, null, null, null, null);
            return layout;
        }

        layout.PageSize = template.PageSize;
        layout.Landscape = template.Landscape;
        layout.BackgroundFileId = template.BackgroundFileId;
        layout.ShowFrameWithoutBackground = template.ElementsJson == null;
        layout.Elements = ElementsOf(template);
        return layout;
    }

    public static List<CertificateElementDto> BuildStandardElements(
        string? signerName,
        string? signerPosition,
        Guid? signatureFileId,
        string? secondSignerName,
        string? secondSignerPosition,
        Guid? secondSignatureFileId)
    {
        List<CertificateElementDto> elements = new List<CertificateElementDto>();
        elements.Add(Text(IssuerLine, 8, 9, 84, 7, 24, true, "#000000"));
        elements.Add(Text("เกียรติบัตร", 8, 17, 84, 14, 50, true, "#0f2854"));
        elements.Add(Text("ขอมอบเกียรติบัตรฉบับนี้ให้ไว้เพื่อแสดงว่า", 8, 33, 84, 8, 26, false, "#000000"));
        elements.Add(Text(CertificateVariables.FullName, 8, 41, 84, 10, 38, true, "#1c5bb8"));
        elements.Add(Text("ได้เข้าร่วมการอบรมหลักสูตร", 8, 52, 84, 8, 26, false, "#000000"));
        elements.Add(Text(CertificateVariables.CourseTitle, 8, 59, 84, 9, 30, true, "#000000"));
        elements.Add(Text("เมื่อวันที่ " + CertificateVariables.CourseDates + " " + CertificateVariables.Hours, 8, 68, 84, 8, 24, false, "#000000"));

        bool hasSecond = !string.IsNullOrWhiteSpace(secondSignerName);
        if (!string.IsNullOrWhiteSpace(signerName))
        {
            double firstX = hasSecond ? 8 : 33;
            double signerWidth = hasSecond ? 40 : 34;
            AddSigner(elements, firstX, signerWidth, signatureFileId, signerName, signerPosition);
            if (hasSecond)
            {
                AddSigner(elements, 52, signerWidth, secondSignatureFileId, secondSignerName!, secondSignerPosition);
            }
        }

        return elements;
    }

    private static void AddSigner(List<CertificateElementDto> elements, double x, double width, Guid? signatureFileId, string signerName, string? signerPosition)
    {
        if (signatureFileId.HasValue)
        {
            CertificateElementDto image = new CertificateElementDto();
            image.Type = "image";
            image.X = x;
            image.Y = 76;
            image.Width = width;
            image.Height = 9;
            image.ImageFileId = signatureFileId;
            elements.Add(image);
        }

        elements.Add(Text("(" + signerName + ")", x, 86, width, 5, 20, true, "#000000"));
        elements.Add(Text(signerPosition ?? string.Empty, x, 91, width, 5, 18, false, "#000000"));
    }

    private static CertificateElementDto Text(string text, double x, double y, double width, double height, double fontSize, bool bold, string color)
    {
        CertificateElementDto element = new CertificateElementDto();
        element.Type = "text";
        element.Text = text;
        element.X = x;
        element.Y = y;
        element.Width = width;
        element.Height = height;
        element.FontSize = fontSize;
        element.Bold = bold;
        element.Color = color;
        element.Align = "center";
        return element;
    }
}
