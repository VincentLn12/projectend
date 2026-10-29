using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers;

/// <summary>
/// อัปโหลด ดาวน์โหลด และลบไฟล์ (รูปภาพและ PDF)
/// </summary>
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// อัปโหลดไฟล์ JPG, PNG, GIF, WebP หรือ PDF (ไม่เกิน 10 MB)
    /// นำ id ที่ได้ไปใส่ตอนบันทึกข่าว โครงงาน ฯลฯ
    /// </summary>
    [HttpPost]
    [Authorize]
    [RequestSizeLimit(FileRules.MaxRequestBytes)]
    public async Task<ActionResult<FileDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        using (Stream stream = file.OpenReadStream())
        {
            FileDto created = await _fileService.UploadAsync(stream, file.FileName, cancellationToken);
            return CreatedAtAction(nameof(Download), new { id = created.Id }, created);
        }
    }

    /// <summary>
    /// เปิดไฟล์ (เปิดสาธารณะ เพราะรูปและ PDF ต้องแสดงบนหน้าเว็บสาธารณะ รหัสไฟล์เป็น GUID เดายาก)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        FileContent fileContent = await _fileService.OpenAsync(id, cancellationToken);
        return File(fileContent.Content, fileContent.ContentType, enableRangeProcessing: true);
    }

    /// <summary>
    /// ลบไฟล์ (เฉพาะ Admin) ลบไม่ได้ถ้ายังมีข้อมูลอื่นใช้ไฟล์นี้อยู่
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _fileService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
