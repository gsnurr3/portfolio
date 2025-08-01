using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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