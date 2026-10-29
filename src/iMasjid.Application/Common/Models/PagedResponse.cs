namespace iMasjid.Application.Common.Models;

public class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResponse(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PagedResponse<T> Create(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        return new PagedResponse<T>(items, count, pageNumber, pageSize);
    }
}
