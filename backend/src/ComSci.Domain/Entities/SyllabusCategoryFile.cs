namespace ComSci.Domain.Entities;

public class SyllabusCategoryFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SyllabusId { get; set; }
    public Syllabus Syllabus { get; set; } = null!;
    public byte SubjectCategory { get; set; }
    public Guid FileId { get; set; }
    public FileRecord File { get; set; } = null!;
}
