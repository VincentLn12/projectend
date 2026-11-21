using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.ShortCourses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/short-courses")]
public class AdminShortCoursesController : ControllerBase
{
    private readonly IShortCourseService _shortCourseService;
    private readonly IShortCourseRegistrationService _registrationService;
    private readonly IShortCourseDocumentService _documentService;

    public AdminShortCoursesController(
        IShortCourseService shortCourseService,
        IShortCourseRegistrationService registrationService,
        IShortCourseDocumentService documentService)
    {
        _shortCourseService = shortCourseService;
        _registrationService = registrationService;
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminShortCourseListItemDto>>> GetList(
        [FromQuery] AdminShortCourseQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminShortCourseListItemDto> result = await _shortCourseService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminShortCourseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminShortCourseDto result = await _shortCourseService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminShortCourseDto>> Create(SaveShortCourseRequest request, CancellationToken cancellationToken)
    {
        AdminShortCourseDto created = await _shortCourseService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminShortCourseDto>> Update(
        Guid id,
        SaveShortCourseRequest request,
        CancellationToken cancellationToken)
    {
        AdminShortCourseDto updated = await _shortCourseService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminShortCourseDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminShortCourseDto updated = await _shortCourseService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _shortCourseService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/registrations")]
    public async Task<ActionResult<List<AdminShortCourseRegistrationDto>>> GetRegistrations(Guid id, CancellationToken cancellationToken)
    {
        List<AdminShortCourseRegistrationDto> result = await _registrationService.GetByShortCourseAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/sign-in-sheet")]
    public async Task<IActionResult> DownloadSignInSheet(Guid id, CancellationToken cancellationToken)
    {
        byte[] pdf = await _documentService.CreateSignInSheetAsync(id, cancellationToken);
        return File(pdf, "application/pdf", "sign-in-sheet-" + id + ".pdf");
    }

    [HttpGet("{id:guid}/certificates")]
    public async Task<IActionResult> DownloadCertificates(Guid id, CancellationToken cancellationToken)
    {
        byte[] pdf = await _documentService.CreateCertificatesForCourseAsync(id, cancellationToken);
        return File(pdf, "application/pdf", "certificates-" + id + ".pdf");
    }

    [HttpGet("{id:guid}/registrations/{registrationId:guid}/certificate")]
    public async Task<IActionResult> DownloadCertificate(Guid id, Guid registrationId, CancellationToken cancellationToken)
    {
        byte[] pdf = await _documentService.CreateCertificateForRegistrationAsync(id, registrationId, cancellationToken);
        return File(pdf, "application/pdf", "certificate-" + registrationId + ".pdf");
    }

    [HttpPost("{id:guid}/registrations/{registrationId:guid}/cancel")]
    public async Task<IActionResult> CancelRegistration(Guid id, Guid registrationId, CancellationToken cancellationToken)
    {
        await _registrationService.CancelByAdminAsync(id, registrationId, cancellationToken);
        return NoContent();
    }
}
