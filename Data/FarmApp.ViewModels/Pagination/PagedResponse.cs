namespace FarmApp.ViewModels.Pagination;

public class PagedResponse<T> where T : class
{
    public List<T> Data { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }

}