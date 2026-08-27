namespace FarmApp.ViewModels.Pagination;

public class PagedFilterModel
{
    public PagedFilterModel()
    {
        PageNumber = 1;
        PageSize = 10;
    }

    public PagedFilterModel(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
