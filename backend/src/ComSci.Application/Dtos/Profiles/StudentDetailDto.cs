using ComSci.Application.Dtos.JobHistories;
using ComSci.Application.Dtos.Projects;
using ComSci.Application.Dtos.StudentProjects;

namespace ComSci.Application.Dtos.Profiles;

public class StudentDetailDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public Guid? ImageFileId { get; set; }
    public short? EntryYear { get; set; }
    public string? Email { get; set; }
    public List<ProjectListItemDto> Projects { get; set; } = new List<ProjectListItemDto>();
    public List<StudentProjectListItemDto> StudentProjects { get; set; } = new List<StudentProjectListItemDto>();
    public List<JobHistoryListItemDto> Jobs { get; set; } = new List<JobHistoryListItemDto>();
}
