namespace NestNet.Infra.BaseClasses
{
    public class DataWithOptionalError<TData>
    {
        public TData? Data { get; set; }
        public string? Error { get; set; }
    }
}
