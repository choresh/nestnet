namespace NestNet.Infra.Paginatation
{
    public class FilterCriteria : BaseCriteria
    {
        public FilterOperator Operator { get; set; }
        public required string Value { get; set; }
    }
}
