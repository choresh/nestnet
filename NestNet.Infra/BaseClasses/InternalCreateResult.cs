namespace NestNet.Infra.BaseClasses
{
    public class InternalCreateResult<TResultDto>
    {
        public required TResultDto ResultDto { get; set; }
        public long Id { get; set; }
    }
}
