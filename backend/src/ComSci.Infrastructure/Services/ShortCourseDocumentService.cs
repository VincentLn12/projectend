using System.Reflection;
using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.CertificateTemplates;
using ComSci.Application.Dtos.ShortCourses;
using ComSci.Application.Exceptions;
using ComSci.Application.Helpers;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ComSci.Infrastructure.Services;

public class ShortCourseDocumentService : IShortCourseDocumentService
{
    private const string FontFamily = "TH Sarabun New";
    private const string RegistrationNotFoundMessage = "ไม่พบข้อมูลการสมัคร กรุณาตรวจสอบเบอร์โทรอีกครั้ง";

    private readonly AppDbContext _dbContext;
    private readonly CertificateTemplateRenderer _certificateRenderer;

    public ShortCourseDocumentService(AppDbContext dbContext, CertificateTemplateRenderer certificateRenderer)
    {
        _dbContext = dbContext;
        _certificateRenderer = certificateRenderer;
    }

    public async Task<byte[]> CreateSignInSheetAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        ShortCourse shortCourse = await LoadCourseAsync(shortCourseId, cancellationToken);

        List<ShortCourseRegistration> registrations = await LoadActiveRegistrationsAsync(shortCourseId, cancellationToken);

        PrepareQuestPdf();

        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(style => style.FontFamily(FontFamily).FontSize(16));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("ใบลงชื่อเข้าร่วมอบรม").FontSize(24).Bold();
                    column.Item().AlignCenter().Text(shortCourse.Title).FontSize(20).Bold();
                    column.Item().AlignCenter().Text("วันที่ " + ThaiDateFormatter.FormatRange(shortCourse.StartDate, shortCourse.EndDate));
                    if (!string.IsNullOrWhiteSpace(shortCourse.Location))
                    {
                        column.Item().AlignCenter().Text("สถานที่ " + shortCourse.Location);
                    }
                    column.Item().PaddingBottom(10);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(36);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).AlignCenter().Text("ที่").Bold();
                        header.Cell().Element(HeaderCell).Text("ชื่อ-นามสกุล").Bold();
                        header.Cell().Element(HeaderCell).Text("เบอร์โทร").Bold();
                        header.Cell().Element(HeaderCell).Text("ลายมือชื่อ").Bold();
                    });

                    for (int index = 0; index < registrations.Count; index++)
                    {
                        ShortCourseRegistration registration = registrations[index];
                        table.Cell().Element(BodyCell).AlignCenter().Text((index + 1).ToString());
                        table.Cell().Element(BodyCell).Text(BuildFullName(registration));
                        table.Cell().Element(BodyCell).Text(registration.Phone);
                        table.Cell().Element(BodyCell).Text(string.Empty);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("หน้า ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> CreateCertificatesForCourseAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        ShortCourse shortCourse = await LoadCourseAsync(shortCourseId, cancellationToken);
        List<ShortCourseRegistration> registrations = await LoadActiveRegistrationsAsync(shortCourseId, cancellationToken);
        if (registrations.Count == 0)
        {
            throw new BadRequestException("ยังไม่มีผู้สมัครที่จะออกเกียรติบัตร");
        }

        List<string> names = registrations.Select(BuildFullName).ToList();
        return await BuildCertificatesAsync(shortCourse, names, cancellationToken);
    }

    public async Task<byte[]> CreateCertificateForRegistrationAsync(Guid shortCourseId, Guid registrationId, CancellationToken cancellationToken)
    {
        ShortCourse shortCourse = await LoadCourseAsync(shortCourseId, cancellationToken);

        ShortCourseRegistration? registration = await _dbContext.ShortCourseRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == registrationId
                    && item.ShortCourseId == shortCourseId
                    && item.Status != RegistrationStatus.Cancelled,
                cancellationToken);
        if (registration == null)
        {
            throw new NotFoundException("ไม่พบข้อมูลการสมัคร");
        }

        return await BuildCertificatesAsync(shortCourse, new List<string> { BuildFullName(registration) }, cancellationToken);
    }

    public async Task<byte[]> CreateCertificateForOwnerAsync(ShortCourseCertificateRequest request, CancellationToken cancellationToken)
    {
        string? phone = PhoneNumberNormalizer.Normalize(request.Phone);
        if (phone == null)
        {
            throw new NotFoundException(RegistrationNotFoundMessage);
        }

        ShortCourseRegistration? registration = await _dbContext.ShortCourseRegistrations
            .AsNoTracking()
            .Include(item => item.ShortCourse)
            .FirstOrDefaultAsync(item => item.Id == request.RegistrationId && item.Phone == phone, cancellationToken);
        if (registration == null)
        {
            throw new NotFoundException(RegistrationNotFoundMessage);
        }

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            throw new BadRequestException("การสมัครนี้ถูกยกเลิกแล้ว จึงไม่มีเกียรติบัตร");
        }

        if (!registration.ShortCourse.IsPublished)
        {
            throw new NotFoundException(RegistrationNotFoundMessage);
        }

        if (DateTime.UtcNow < registration.ShortCourse.EndDate)
        {
            throw new BadRequestException("เกียรติบัตรจะดาวน์โหลดได้หลังสิ้นสุดการอบรม");
        }

        return await BuildCertificatesAsync(registration.ShortCourse, new List<string> { BuildFullName(registration) }, cancellationToken);
    }

    private async Task<ShortCourse> LoadCourseAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        ShortCourse? shortCourse = await _dbContext.ShortCourses
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == shortCourseId, cancellationToken);
        if (shortCourse == null)
        {
            throw new NotFoundException("ไม่พบหลักสูตรนี้");
        }

        return shortCourse;
    }

    private async Task<List<ShortCourseRegistration>> LoadActiveRegistrationsAsync(Guid shortCourseId, CancellationToken cancellationToken)
    {
        return await _dbContext.ShortCourseRegistrations
            .AsNoTracking()
            .Where(item => item.ShortCourseId == shortCourseId && item.Status != RegistrationStatus.Cancelled)
            .OrderBy(item => item.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    private async Task<byte[]> BuildCertificatesAsync(ShortCourse shortCourse, List<string> names, CancellationToken cancellationToken)
    {
        CertificateTemplate? template = await FindTemplateAsync(shortCourse, cancellationToken);
        CertificateLayoutDto layout = CertificateLayoutDefaults.ResolveLayout(template);

        List<IReadOnlyDictionary<string, string>> variableSets = names
            .Select(name => CertificateVariables.ForCourse(shortCourse, name))
            .ToList();

        return await _certificateRenderer.RenderAsync(layout, variableSets, cancellationToken);
    }

    // ใช้แม่แบบของหลักสูตร ถ้ายังไม่ได้เลือกใช้แม่แบบล่าสุดที่สร้างไว้ ถ้ายังไม่มีเลยจะออกเกียรติบัตรแบบกรอบเรียบไม่มีผู้ลงนาม
    private async Task<CertificateTemplate?> FindTemplateAsync(ShortCourse shortCourse, CancellationToken cancellationToken)
    {
        IQueryable<CertificateTemplate> templates = _dbContext.CertificateTemplates.AsNoTracking();

        if (shortCourse.CertificateTemplateId.HasValue)
        {
            CertificateTemplate? chosen = await templates
                .FirstOrDefaultAsync(item => item.Id == shortCourse.CertificateTemplateId.Value, cancellationToken);
            if (chosen != null)
            {
                return chosen;
            }
        }

        return await templates
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string BuildFullName(ShortCourseRegistration registration)
    {
        string prefix = string.IsNullOrWhiteSpace(registration.Prefix) ? string.Empty : registration.Prefix.Trim();
        return (prefix + registration.FirstName + " " + registration.LastName).Trim();
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container.Border(1).BorderColor(Colors.Grey.Darken2).Background(Colors.Grey.Lighten3).Padding(4);
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container.Border(1).BorderColor(Colors.Grey.Darken2).MinHeight(30).PaddingHorizontal(4).PaddingVertical(2).AlignMiddle();
    }

    private static void PrepareQuestPdf()
    {
        CertificateTemplateRenderer.PrepareQuestPdf();
    }
}
