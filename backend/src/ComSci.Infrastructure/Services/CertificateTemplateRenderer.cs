using System.Reflection;
using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.CertificateTemplates;
using ComSci.Application.Helpers;
using ComSci.Domain.Entities;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ComSci.Infrastructure.Services;

public class CertificateTemplateRenderer
{
    private const string FontFamily = "TH Sarabun New";

    private static readonly object FontLock = new object();
    private static bool _fontsRegistered;

    private readonly AppDbContext _dbContext;
    private readonly IFileStorage _fileStorage;

    public CertificateTemplateRenderer(AppDbContext dbContext, IFileStorage fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<byte[]> RenderAsync(
        CertificateLayoutDto layout,
        IReadOnlyList<IReadOnlyDictionary<string, string>> variableSets,
        CancellationToken cancellationToken)
    {
        PrepareQuestPdf();

        Dictionary<Guid, byte[]> images = await LoadImagesAsync(layout, cancellationToken);
        byte[]? background = null;
        if (layout.BackgroundFileId.HasValue)
        {
            images.TryGetValue(layout.BackgroundFileId.Value, out background);
        }

        PageSize pageSize = ResolvePageSize(layout);

        Document document = Document.Create(container =>
        {
            foreach (IReadOnlyDictionary<string, string> variables in variableSets)
            {
                container.Page(page => ComposePage(page, layout, pageSize, background, images, variables));
            }
        });

        return document.GeneratePdf();
    }

    public static void PrepareQuestPdf()
    {
        lock (FontLock)
        {
            if (_fontsRegistered)
            {
                return;
            }

            QuestPDF.Settings.License = LicenseType.Community;

            Assembly assembly = typeof(CertificateTemplateRenderer).Assembly;
            string[] fontResourceNames = assembly.GetManifestResourceNames()
                .Where(resourceName => resourceName.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            foreach (string resourceName in fontResourceNames)
            {
                using (Stream? fontStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (fontStream != null)
                    {
                        FontManager.RegisterFont(fontStream);
                    }
                }
            }

            _fontsRegistered = true;
        }
    }

    private static void ComposePage(
        PageDescriptor page,
        CertificateLayoutDto layout,
        PageSize pageSize,
        byte[]? background,
        Dictionary<Guid, byte[]> images,
        IReadOnlyDictionary<string, string> variables)
    {
        page.Size(pageSize);
        page.Margin(0);
        page.DefaultTextStyle(style => style.FontFamily(FontFamily).FontSize(24));

        page.Content().Layers(layers =>
        {
            layers.PrimaryLayer().Element(container => ComposeBackground(container, layout, background));

            foreach (CertificateElementDto element in layout.Elements)
            {
                float left = (float)(element.X / 100 * pageSize.Width);
                float top = (float)(element.Y / 100 * pageSize.Height);
                float width = (float)(element.Width / 100 * pageSize.Width);
                float height = (float)(element.Height / 100 * pageSize.Height);

                layers.Layer()
                    .TranslateX(left)
                    .TranslateY(top)
                    .Width(width)
                    .Height(height)
                    .Element(container => ComposeElement(container, element, images, variables));
            }
        });
    }

    private static void ComposeBackground(IContainer container, CertificateLayoutDto layout, byte[]? background)
    {
        if (background != null)
        {
            container.Image(background).FitUnproportionally();
        }
        else if (layout.ShowFrameWithoutBackground)
        {
            container.Padding(24).Border(3).BorderColor("#0f2854");
        }
        else
        {
            container.Background(Colors.White);
        }
    }

    private static void ComposeElement(
        IContainer container,
        CertificateElementDto element,
        Dictionary<Guid, byte[]> images,
        IReadOnlyDictionary<string, string> variables)
    {
        if (element.Type == "image")
        {
            if (element.ImageFileId.HasValue && images.TryGetValue(element.ImageFileId.Value, out byte[]? bytes))
            {
                container.AlignCenter().AlignMiddle().Image(bytes).FitArea();
            }

            return;
        }

        string value = CertificateVariables.Apply(element.Text ?? string.Empty, variables).Replace("\r\n", "\n").Trim();
        string[] lines = value.Split('\n');

        container.Text(text =>
        {
            if (element.Align == "left")
            {
                text.AlignLeft();
            }
            else if (element.Align == "right")
            {
                text.AlignRight();
            }
            else
            {
                text.AlignCenter();
            }

            foreach (string line in lines)
            {
                TextSpanDescriptor span = text.Line(line).FontSize((float)element.FontSize).FontColor(element.Color);
                if (element.Bold)
                {
                    span = span.Bold();
                }

                if (element.Italic)
                {
                    span = span.Italic();
                }

                if (element.Underline)
                {
                    span.Underline();
                }
            }
        });
    }

    private static PageSize ResolvePageSize(CertificateLayoutDto layout)
    {
        PageSize size = layout.PageSize switch
        {
            "A5" => PageSizes.A5,
            "Letter" => PageSizes.Letter,
            _ => PageSizes.A4
        };

        return layout.Landscape ? size.Landscape() : size;
    }

    private async Task<Dictionary<Guid, byte[]>> LoadImagesAsync(CertificateLayoutDto layout, CancellationToken cancellationToken)
    {
        List<Guid> wantedIds = layout.Elements
            .Where(element => element.Type == "image" && element.ImageFileId.HasValue)
            .Select(element => element.ImageFileId!.Value)
            .ToList();
        if (layout.BackgroundFileId.HasValue)
        {
            wantedIds.Add(layout.BackgroundFileId.Value);
        }

        List<Guid> distinctIds = wantedIds.Distinct().ToList();
        List<FileRecord> files = await _dbContext.Files
            .AsNoTracking()
            .Where(file => distinctIds.Contains(file.Id))
            .ToListAsync(cancellationToken);

        Dictionary<Guid, byte[]> images = new Dictionary<Guid, byte[]>();
        foreach (FileRecord file in files)
        {
            using (Stream? stream = _fileStorage.OpenRead(file.StoragePath))
            {
                if (stream == null)
                {
                    continue;
                }

                using (MemoryStream buffer = new MemoryStream())
                {
                    await stream.CopyToAsync(buffer, cancellationToken);
                    images[file.Id] = buffer.ToArray();
                }
            }
        }

        return images;
    }
}
