namespace NestNet.Infra.BaseClasses
{
    public interface IEntity
    {
        // * This property expose DB auto generated ID.
        // * This property enables code at 'NestNet.Infra' to handle the entity
        //   in general manner (without knowing the specific name which defined
        //   in the derived class (i.e. in the entity class).
        // * The property will be implemented by derived class (i.e. the entity class).
        int Id { get; set; }
    }
}
