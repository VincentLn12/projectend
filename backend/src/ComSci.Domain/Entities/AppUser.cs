using ComSci.Domain.Common;
using ComSci.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ComSci.Domain.Entities;

public class AppUser : IdentityUser<Guid>, IAuditable
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public Guid? ImageFileId { get; set; }
    public FileRecord? ImageFile { get; set; }
    public UserType UserType { get; set; } = UserType.General;
    public bool ShowEmail { get; set; }
    public bool ShowProfile { get; set; }
    public bool ShowPhone { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? Skills { get; set; }
    public DateTime? ConsentAt { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public StudentProfile? StudentProfile { get; set; }
    public ICollection<JobHistory> JobHistories { get; set; } = new List<JobHistory>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
