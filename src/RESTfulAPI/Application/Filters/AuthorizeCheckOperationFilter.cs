using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RESTfulAPI.Application.Filters
{
    /// <summary>
    /// Adds 401 and 403 response documentation and OAuth2 security requirement
    /// to any OpenAPI operation whose controller or action is decorated with [Authorize].
    /// </summary>
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext ctx)
        {
            // Does this endpoint (or its controller) require [Authorize]?
            var hasAuth =
                ctx.MethodInfo.DeclaringType!.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                ctx.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!hasAuth) return;

            operation.Responses.TryAdd("401", new() { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new() { Description = "Forbidden" });

            var scheme = new OpenApiSecurityScheme
            {
                Reference = new() { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            };
            operation.Security.Add(new OpenApiSecurityRequirement { [scheme] = new List<string>() });
        }
    }
}
