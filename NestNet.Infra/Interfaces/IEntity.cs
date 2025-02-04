namespace NestNet.Infra.Interfaces
{
    public interface IEntity
    {
        // * This property expose DB auto generated ID.
        // * This property enables code at 'NestNet.Infra' to handle the entity
        //   in general manner (without knowing the specific name which defined
        //   in the derived class - the actual entity class).
        // * The property will be implemented by derived class (the actual entity class).
        long Id { get; set; }
    }
}
