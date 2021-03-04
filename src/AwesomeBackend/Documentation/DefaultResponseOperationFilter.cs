using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Net.Mime;

namespace AwesomeBackend.Documentation
{
    public class DefaultResponseOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses.TryAdd("default", GetResponse("Error"));
        }

        private static OpenApiResponse GetResponse(string description)
           => new()
           {
               Description = description,
               Content = new Dictionary<string, OpenApiMediaType>
               {
                   [MediaTypeNames.Application.Json] = new OpenApiMediaType
                   {
                       Schema = new OpenApiSchema
                       {
                           Reference = new OpenApiReference
                           {
                               Id = nameof(ProblemDetails),
                               Type = ReferenceType.Schema
                           }
                       }
                   }
               }
           };
    }
}
