namespace NestNet.Infra.Query;

public abstract class PaginationInput
{
    public int? Skip { get; set; }

    public int? Take { get; set; }
}
