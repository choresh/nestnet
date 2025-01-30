#pragma warning disable IDE0290 // Use primary constructor

namespace NestNet.Infra.Attributes
{
    public enum GenOpt
    {
        Ignore,
        Optional,
        Mandatory
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropAttribute : Attribute
    {
        public GenOpt Create { get; }
        public GenOpt Update { get; }
        public GenOpt Result { get; }

        public PropAttribute(GenOpt create, GenOpt update, GenOpt result)
        {
            Create = create;
            Update = update;
            Result = result;
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor
