using System.ComponentModel.DataAnnotations;

namespace ComSci.Application.Dtos.Menu;

public class ReorderMenuRequest
{
    public Guid? ParentId { get; set; }
    [Required]
    public List<Guid> OrderedIds { get; set; } = new List<Guid>();
}
