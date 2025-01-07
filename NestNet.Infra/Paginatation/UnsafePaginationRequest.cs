namespace NestNet.Infra.Paginatation
{
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
