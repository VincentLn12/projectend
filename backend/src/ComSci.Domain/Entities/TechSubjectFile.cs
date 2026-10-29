namespace ComSci.Domain.Entities;

public class TechSubjectFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TechSubjectId { get; set; }
    public TechSubject TechSubject { get; set; } = null!;
    public Guid FileId { get; set; }
    public FileRecord File { get; set; } = null!;
    public int SortOrder { get; set; }
}
