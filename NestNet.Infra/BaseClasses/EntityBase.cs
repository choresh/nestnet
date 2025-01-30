namespace NestNet.Infra.BaseClasses
{
    public abstract class EntityBase : IEntity
    {
        public abstract long Id { get; set; }
    }
}
