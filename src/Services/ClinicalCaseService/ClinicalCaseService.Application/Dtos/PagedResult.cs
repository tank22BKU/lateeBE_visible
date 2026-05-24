public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public object? Filters { get; set; }

    public int TotalPages =>
        (int)Math.Ceiling((double)Total / PageSize);
}

public class ClinicalCaseListFilters
{
    public List<string> AvailableStatuses { get; set; } = [];
    public List<string> AvailableTypes { get; set; } = [];
    public List<string> AvailableEccids { get; set; } = [];
}
