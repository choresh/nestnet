#pragma warning disable IDE0290 // Use primary constructor
using System.ComponentModel.DataAnnotations.Schema;

namespace NestNet.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EntityAttribute : Attribute
    {
        private readonly TableAttribute _tableAttribute;

        public EntityAttribute(string tableName)
        {
            _tableAttribute = new TableAttribute(tableName);
        }

        public string Name => _tableAttribute.Name;

        public string? Schema => _tableAttribute.Schema;
    }
}

#pragma warning restore IDE0290 // Use primary constructor

