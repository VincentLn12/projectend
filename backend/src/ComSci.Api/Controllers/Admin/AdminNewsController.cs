using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Controllers.Admin;

/// <summary>
/// จัดการข่าวสำหรับผู้ดูแลระบบ (ต้องล็อกอินด้วย role Admin)
/// </summary>
[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/news")]
public class AdminNewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public AdminNewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    /// <summary>
    /// รายการข่าวทั้งหมดรวมฉบับร่าง กรองสถานะด้วย ?isPublished=true|false
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminNewsListItemDto>>> GetList(
        [FromQuery] AdminNewsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<AdminNewsListItemDto> result = await _newsService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// รายละเอียดข่าวหนึ่งรายการ (ดูฉบับร่างได้)
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminNewsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        AdminNewsDto result = await _newsService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// สร้างข่าวใหม่
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AdminNewsDto>> Create(SaveNewsRequest request, CancellationToken cancellationToken)
    {
        AdminNewsDto created = await _newsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// แก้ไขข่าว (ส่งข้อมูลครบทุกฟิลด์ รวมรายการไฟล์ที่ต้องการให้เหลืออยู่)
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminNewsDto>> Update(
        Guid id,
        SaveNewsRequest request,
        CancellationToken cancellationToken)
    {
        AdminNewsDto updated = await _newsService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// เผยแพร่ข่าว หรือถอนกลับเป็นฉบับร่าง
    /// </summary>
    [HttpPatch("{id:guid}/published")]
    public async Task<ActionResult<AdminNewsDto>> SetPublished(
        Guid id,
        SetPublishedRequest request,
        CancellationToken cancellationToken)
    {
        AdminNewsDto updated = await _newsService.SetPublishedAsync(id, request.IsPublished, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// ลบข่าว (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _newsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
