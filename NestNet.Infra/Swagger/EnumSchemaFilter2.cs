using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace NestNet.Infra.Swagger
{
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
}

