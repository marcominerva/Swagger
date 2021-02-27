using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;

namespace AwesomeBackend.Documentation
{
    public class AuthResponseOperationFilter : IOperationFilter
    {
        private readonly IAuthorizationPolicyProvider authorizationPolicyProvider;

        public AuthResponseOperationFilter(IAuthorizationPolicyProvider authorizationPolicyProvider)
            => this.authorizationPolicyProvider = authorizationPolicyProvider;

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fallbackPolicy = authorizationPolicyProvider.GetFallbackPolicyAsync().GetAwaiter().GetResult();
            var requireAuthenticatedUser = fallbackPolicy?.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement) ?? false;

            var requireAuthorization = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .Any(a => a is AuthorizeAttribute) ?? false;

            var allowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .Any(a => a is AllowAnonymousAttribute) ?? false;

            if ((requireAuthenticatedUser || requireAuthorization) && !allowAnonymous)
            {
                operation.Responses.TryAdd(StatusCodes.Status401Unauthorized.ToString(), GetResponse(HttpStatusCode.Unauthorized.ToString()));
            }
        }

        private static OpenApiResponse GetResponse(string description)
            => new OpenApiResponse
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
