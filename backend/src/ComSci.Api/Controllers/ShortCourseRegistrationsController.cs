using ComSci.Application.Abstractions;
using ComSci.Application.Dtos.ShortCourses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComSci.Api.Controllers;

[ApiController]
[EnableRateLimiting("public-registration")]
[Route("api/short-course-registrations")]
public class ShortCourseRegistrationsController : ControllerBase
{
    private readonly IShortCourseRegistrationService _registrationService;
    private readonly IShortCourseDocumentService _documentService;

    public ShortCourseRegistrationsController(
        IShortCourseRegistrationService registrationService,
        IShortCourseDocumentService documentService)
    {
        _registrationService = registrationService;
        _documentService = documentService;
    }

    [HttpPost("lookup")]
    public async Task<ActionResult<List<ShortCourseRegistrationDto>>> Lookup(
        LookupShortCourseRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        List<ShortCourseRegistrationDto> result = await _registrationService.LookupAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("certificate")]
    public async Task<IActionResult> DownloadCertificate(
        ShortCourseCertificateRequest request,
        CancellationToken cancellationToken)
    {
        byte[] pdf = await _documentService.CreateCertificateForOwnerAsync(request, cancellationToken);
        return File(pdf, "application/pdf", "certificate.pdf");
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShortCourseRegistrationDto>> Update(
        Guid id,
        UpdateShortCourseRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        ShortCourseRegistrationDto result = await _registrationService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancelShortCourseRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        await _registrationService.CancelAsync(id, request, cancellationToken);
        return NoContent();
    }
}
