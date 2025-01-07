namespace NestNet.Infra.Query;

public abstract class QueryRange
{
    public int? Skip { get; set; }

    public int? Take { get; set; }
}
