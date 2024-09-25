namespace NestNet.Infra.Query
{
    public class PaginatedResult<T>
    {
        public required IEnumerable<T> Items { get; set; }
        public int? TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public enum FilterOperator
    {
        Equals,
        NotEquals,
        Contains,
        NotContains,
        GreaterThan,
        LessThan
    }

    public class BaseCriteria
    {
        public required string PropertyName { get; set; }
    }

    public class FilterCriteria: BaseCriteria
    {
        public FilterOperator Operator { get; set; }
        public required string Value { get; set; }
    }

    public enum SortDirection
    {
        Asc,
        Desc
    }

    public class SortCriteria: BaseCriteria
    {
        public SortDirection SortDirection { get; set; }
    }

    public class SafePaginationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeTotalCount { get; set; } = false;
        public required IList<SortCriteria> SortCriteria { get; set; }
        public required IList<FilterCriteria> FilterCriteria { get; set; }
    }

    public class UnsafePaginationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeTotalCount { get; set; } = false;
        public string[]? SortBy { get; set; } = null;
        public string[]? SortDirection { get; set; } = null;
        public string[]? FilterBy { get; set; } = null;
        public string[]? FilterOperator { get; set; } = null;
        public string[]? FilterValue { get; set; } = null;
    }
}
