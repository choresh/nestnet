using NestNet.Infra.Attributes;

namespace NestNet.Infra.BaseClasses
{
    public abstract class EntityBase : IEntity
    {
        public abstract int Id { get; set; }

        [Prop(
           create: GenOpt.Ignore,
           update: GenOpt.Ignore,
           result: GenOpt.Mandatory,
           store: DbOpt.Standard
        )]
        public DateTime CreatedAt { get; set; }

        [Prop(
          create: GenOpt.Ignore,
          update: GenOpt.Ignore,
          result: GenOpt.Mandatory,
          store: DbOpt.Standard
        )]
        public DateTime UpdatedAt { get; set; }
    }
}
