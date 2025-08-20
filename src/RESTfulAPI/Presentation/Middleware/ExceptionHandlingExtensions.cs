using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using RESTfulAPI.Presentation.Common;

namespace RESTfulAPI.Presentation.Middleware;

/// <summary>
/// Adds a single gateway for all unhandled exceptions (MVC, MediatR,
/// static-file pipeline, everything) and converts them into ApiResponse envelopes.
/// </summary>
public static class ExceptionHandlingExtensions
{
    public static void UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appErr =>
        {
            appErr.Run(async ctx =>
            {
                var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
                var env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();

                // Map exception → ApiResponse envelope
                ApiResponse<object?> envelope = ex switch
                {
                    // 422 – FluentValidation
                    ValidationException ve => ApiResponse.ValidationFailed(
                        ve.Errors.Select(e =>
                            new ApiError(e.ErrorCode, e.ErrorMessage, e.PropertyName))),

                    // 404
                    KeyNotFoundException knf => ApiResponse.NotFound(knf.Message),

                    // 403
                    UnauthorizedAccessException ua => ApiResponse.Forbidden(ua.Message),

                    // 400 (argument / domain rule breaches)
                    ArgumentException ae => ApiResponse.BadRequest(ae.Message),

                    // Everything else – 500
                    _ => BuildGenericError(env, ex)
                };

                ctx.Response.StatusCode = envelope.Status;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(envelope);
            });
        });
    }

    // 500 payloads (detailed in Dev, safe in Prod)
    private static ApiResponse<object?> BuildGenericError(IHostEnvironment env, Exception? ex)
        => env.IsDevelopment()
           ? ApiResponse.Error(
                 "Unhandled exception.",
                 new[] { new ApiError("Exception", ex?.ToString() ?? string.Empty) })
           : ApiResponse.Error("An unexpected error occurred. Please contact support.");
}
