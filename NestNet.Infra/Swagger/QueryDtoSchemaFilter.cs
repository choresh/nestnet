using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using NestNet.Infra.Attributes;
using Microsoft.OpenApi.Any;
using NestNet.Infra.Helpers;

namespace NestNet.Infra.Swagger
{
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