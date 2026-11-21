using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.CertificateTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/certificate-templates")]
public class AdminCertificateTemplatesController : ControllerBase
{
    private readonly ICertificateTemplateService _templateService;

    public AdminCertificateTemplatesController(ICertificateTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CertificateTemplateDto>>> GetAll(CancellationToken cancellationToken)
    {
        List<CertificateTemplateDto> result = await _templateService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(SaveCertificateTemplateRequest request, CancellationToken cancellationToken)
    {
        byte[] pdf = await _templateService.CreatePreviewAsync(request, cancellationToken);
        return File(pdf, "application/pdf", "certificate-preview.pdf");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CertificateTemplateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        CertificateTemplateDto result = await _templateService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CertificateTemplateDto>> Create(SaveCertificateTemplateRequest request, CancellationToken cancellationToken)
    {
        CertificateTemplateDto created = await _templateService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CertificateTemplateDto>> Update(
        Guid id,
        SaveCertificateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        CertificateTemplateDto updated = await _templateService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _templateService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
