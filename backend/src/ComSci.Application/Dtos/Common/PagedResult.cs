namespace ComSci.Application.Dtos.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}
