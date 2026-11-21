namespace ComSci.Application.Dtos.Projects;

public class AdminProjectQuery : ProjectQuery
{
    public bool? IsPublished { get; set; }

    public Guid? StudentId { get; set; }
}
