using System.ComponentModel.DataAnnotations;
using ComSci.Application.Dtos.Common;

namespace ComSci.Application.Dtos.News;

// ---- public (no internal fields, only published items) ----
public record NewsListItemDto(Guid Id, string Title, string? SubTitle, DateTime? PublishedAt, Guid? MainImageFileId);

public record NewsDetailDto(
    Guid Id, string Title, string? SubTitle, string Body, DateTime? PublishedAt,
    string AuthorName, IReadOnlyList<Guid> ImageFileIds);

// ---- admin ----
public class AdminNewsQuery : PageQuery
{
    public bool? IsPublished { get; set; }
}

public record AdminNewsListItemDto(
    Guid Id, string Title, DateTime? PublishedAt, bool IsPublished, string AuthorName, DateTime CreatedAt);

public record NewsFileDto(Guid FileId, bool IsMain, int SortOrder);

public record AdminNewsDto(
    Guid Id, string Title, string? SubTitle, string Body, DateTime? PublishedAt, bool IsPublished,
    string AuthorName, DateTime CreatedAt, DateTime? UpdatedAt, IReadOnlyList<NewsFileDto> Files);

public record SaveNewsRequest(
    [Required, StringLength(300)] string Title,
    [StringLength(500)] string? SubTitle,
    [Required] string Body,
    bool IsPublished,
    IReadOnlyList<Guid>? FileIds,
    Guid? MainFileId);

public record SetPublishedRequest(bool IsPublished);
