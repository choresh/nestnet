namespace NestNet.Infra.Paginatation
{
    public class SafePaginationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeTotalCount { get; set; } = false;
        public required IList<SortCriteria> SortCriteria { get; set; }
        public required IList<FilterCriteria> FilterCriteria { get; set; }
    }
}
