[AttributeUsage(AttributeTargets.Parameter)]
public class EnumSchemaAttribute : Attribute
{
    public Type EnumType { get; }

    public EnumSchemaAttribute(Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("Type must be an enum", nameof(enumType));
        }
        EnumType = enumType;
    }
} 