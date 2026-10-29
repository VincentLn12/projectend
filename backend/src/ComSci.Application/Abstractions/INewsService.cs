using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.News;

namespace ComSci.Application.Abstractions;

/// <summary>
/// จัดการข่าวประชาสัมพันธ์ แบ่งเป็นส่วนสาธารณะ (อ่านอย่างเดียว) และส่วนผู้ดูแล
/// </summary>
public interface INewsService
{
    // ---------------- ส่วนสาธารณะ ----------------

    /// <summary>
    /// รายการข่าวที่เผยแพร่แล้ว เรียงจากใหม่ไปเก่า รองรับค้นหาจากหัวข้อ
    /// </summary>
    Task<PagedResult<NewsListItemDto>> GetPublishedAsync(PageQuery pageQuery, CancellationToken cancellationToken);

    /// <summary>
    /// รายละเอียดข่าวที่เผยแพร่แล้ว
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">ไม่พบข่าว หรือข่าวยังไม่เผยแพร่</exception>
    Task<NewsDetailDto> GetPublishedByIdAsync(Guid newsId, CancellationToken cancellationToken);

    // ---------------- ส่วนผู้ดูแล ----------------

    /// <summary>
    /// รายการข่าวทั้งหมดรวมฉบับร่าง
    /// </summary>
    Task<PagedResult<AdminNewsListItemDto>> GetAllAsync(AdminNewsQuery query, CancellationToken cancellationToken);

    /// <exception cref="Exceptions.NotFoundException">ไม่พบข่าว</exception>
    Task<AdminNewsDto> GetByIdAsync(Guid newsId, CancellationToken cancellationToken);

    /// <summary>
    /// สร้างข่าวใหม่ โดยผู้เขียนคือผู้ที่ล็อกอินอยู่
    /// </summary>
    /// <exception cref="Exceptions.BadRequestException">แนบไฟล์ที่ไม่มีอยู่จริง หรือ MainFileId ไม่อยู่ใน FileIds</exception>
    Task<AdminNewsDto> CreateAsync(SaveNewsRequest request, CancellationToken cancellationToken);

    /// <exception cref="Exceptions.NotFoundException">ไม่พบข่าว</exception>
    /// <exception cref="Exceptions.BadRequestException">แนบไฟล์ที่ไม่มีอยู่จริง หรือ MainFileId ไม่อยู่ใน FileIds</exception>
    Task<AdminNewsDto> UpdateAsync(Guid newsId, SaveNewsRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// เผยแพร่ข่าว หรือถอนกลับเป็นฉบับร่าง
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">ไม่พบข่าว</exception>
    Task<AdminNewsDto> SetPublishedAsync(Guid newsId, bool isPublished, CancellationToken cancellationToken);

    /// <summary>
    /// ลบข่าวแบบ soft delete
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">ไม่พบข่าว</exception>
    Task DeleteAsync(Guid newsId, CancellationToken cancellationToken);
}
