using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace NestNet.Infra.Swagger
{
    public class EnumSchemaFilter : ISchemaFilter
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

                // Create items schema for array
                var itemsSchema = new OpenApiSchema
                {
                    Type = "string",
                    Enum = enumValues
                };

                // Set up array schema with MultipleOf to enable better multi-select UI
                schema.Type = "array";
                schema.Items = itemsSchema;
                schema.UniqueItems = true;
                schema.MultipleOf = 1;
            }
        }
    }
}

