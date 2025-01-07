using System;

namespace NestNet.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ModelMetadataTypeAttribute : Attribute
    {
        public Type ModelType { get; }

        public ModelMetadataTypeAttribute(Type modelType)
        {
            ModelType = modelType;
        }
    }
} 