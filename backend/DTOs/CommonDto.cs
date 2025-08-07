namespace CodeSnippetManager.Api.DTOs;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class SnippetFilter
{
    public string? Search { get; set; }
    public string? Language { get; set; }
    public string? Tag { get; set; }
    public Guid? CreatedBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SnippetFilterDto
{
    public string? Search { get; set; }
    public string? Language { get; set; }
    public string? Tag { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}