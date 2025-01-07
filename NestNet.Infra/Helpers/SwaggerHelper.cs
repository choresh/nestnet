using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NestNet.Infra.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace NestNet.Infra.Helpers
{
    public static class SwaggerHelper
    {
        public class EnumSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                if (context.Type.IsEnum)
                {
                    schema.Enum.Clear();
                    schema.Type = "string";
                    schema.Format = null;

                    foreach (var name in Enum.GetNames(context.Type))
                    {
                        schema.Enum.Add(new OpenApiString(name));
                    }
                }
            }
        }

        public class EnumSchemaFilter2 : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                var enumAttribute = context.ParameterInfo?
                    .GetCustomAttribute<EnumSchemaAttribute>();

                if (enumAttribute != null)
                {
                    var enumValues = Enum.GetNames(enumAttribute.EnumType)
                        .Select(name => new OpenApiString(name))
                        .Cast<IOpenApiAny>()
                        .ToList();

                    schema.Enum = enumValues;
                    schema.Type = "string";
                }
            }
        }

        public class QueryDtoSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                var queryDtoMetadata = context.ParameterInfo?
                    .GetCustomAttribute<QueryDtoMetadataAttribute>();

                if (queryDtoMetadata != null)
                {
                    var controllerType = context.ParameterInfo.Member.DeclaringType;
                    var queryDtoType = controllerType.GetGenericArguments()[4]; // TQueryDto is the 5th type parameter

                    var properties = queryDtoType
                        .GetProperties()
                        .Select(p => StringHelper.ToCamelCase(p.Name))
                        .ToList();

                    schema.Enum = properties.Select(p => new OpenApiString(p)).Cast<IOpenApiAny>().ToList();
                    schema.Type = "string";
                }
            }
        }
    }
}
