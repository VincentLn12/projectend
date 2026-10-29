namespace ComSci.Domain.Entities;

public class CoursePhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public Guid FileId { get; set; }
    public FileRecord File { get; set; } = null!;
    public bool IsMain { get; set; }
}
