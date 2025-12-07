using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Elo.Presentation.Configuration;

public class SwaggerFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo
            .GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToList();

        if (fileParameters.Count == 0)
        {
            return;
        }

        operation.RequestBody ??= new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>()
        };

        if (operation.RequestBody.Content == null)
        {
            operation.RequestBody.Content = new Dictionary<string, OpenApiMediaType>();
        }

        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = fileParameters.ToDictionary(
                parameter => parameter.Name ?? "file",
                _ => new OpenApiSchema { Type = "string", Format = "binary" })
        };

        if (schema.Properties.Count > 0)
        {
            schema.Required = new HashSet<string>(schema.Properties.Keys);
        }

        operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
        {
            Schema = schema
        };

        if (operation.Parameters != null)
        {
            foreach (var fileParameter in fileParameters)
            {
                var existing = operation.Parameters.FirstOrDefault(p => p.Name == fileParameter.Name);
                if (existing != null)
                {
                    operation.Parameters.Remove(existing);
                }
            }
        }

        operation.RequestBody.Required = true;
    }
}
