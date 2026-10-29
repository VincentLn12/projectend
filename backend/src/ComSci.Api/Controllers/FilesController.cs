using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController(IFileService files) : ControllerBase
{
    /// <summary>Upload one JPG, PNG, GIF, WebP or PDF (max 10 MB). Use the returned id when saving news, projects, etc.</summary>
    [HttpPost]
    [Authorize]
    [RequestSizeLimit(FileRules.MaxRequestBytes)]
    public async Task<ActionResult<FileDto>> Upload(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var created = await files.UploadAsync(stream, file.FileName, ct);
        return CreatedAtAction(nameof(Download), new { id = created.Id }, created);
    }

    /// <summary>Public: images and PDFs appear on public pages. The id is an unguessable GUID.</summary>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var file = await files.OpenAsync(id, ct);
        return File(file.Content, file.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await files.DeleteAsync(id, ct);
        return NoContent();
    }
}
