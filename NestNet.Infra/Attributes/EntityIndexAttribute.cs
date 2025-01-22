#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NestNet.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EntityIndexAttribute : Attribute
    {
        private readonly IndexAttribute _indexAttribute;

        public EntityIndexAttribute(string propertyName, bool isUnique)
        {
            _indexAttribute = new IndexAttribute(propertyName);
            _indexAttribute.IsUnique = isUnique;
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor

