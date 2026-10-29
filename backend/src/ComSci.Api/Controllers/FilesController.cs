using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("upload")]
    [RequestSizeLimit(FileRules.MaxRequestBytes)]
    public async Task<ActionResult<FileDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        using (Stream stream = file.OpenReadStream())
        {
            FileDto created = await _fileService.UploadAsync(stream, file.FileName, cancellationToken);
            return CreatedAtAction(nameof(Download), new { id = created.Id }, created);
        }
    }

    // เปิดสาธารณะ เพราะรูปและ PDF แสดงบนหน้าสาธารณะ (รหัสเป็น GUID เดายาก)
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        FileContent fileContent = await _fileService.OpenAsync(id, cancellationToken);
        return File(fileContent.Content, fileContent.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _fileService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
