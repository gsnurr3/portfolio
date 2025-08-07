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
                // Grab the thrown exception (null if somehow missing)
                var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;

                // Get helpers from the DI container
                var env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();
                var log = ctx.RequestServices.GetRequiredService<ILoggerFactory>()
                                             .CreateLogger("GlobalException");

                // Log first – always do this before mutating the response
                log.LogError(ex, "Unhandled exception");

                // Map exception → ApiResponse envelope
                ApiResponse<object?> envelope = ex switch
                {
                    // FluentValidation – 422
                    ValidationException ve => ApiResponse.ValidationFailed(
                        ve.Errors.Select(e =>
                            new ApiError(e.ErrorCode,
                                         e.ErrorMessage,
                                         e.PropertyName))),

                    // 404
                    KeyNotFoundException knf => ApiResponse.NotFound(knf.Message),

                    // 403
                    UnauthorizedAccessException ua => ApiResponse.Forbidden(ua.Message),

                    // 400 (argument / domain rule breaches)
                    ArgumentException ae => ApiResponse.BadRequest(ae.Message),

                    // Everything else - 500
                    _ => BuildGenericError(env, ex)
                };

                // Craft the HTTP response
                ctx.Response.StatusCode = envelope.Status;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(envelope);
            });
        });
    }

    // Helper for 500 payloads
    private static ApiResponse<object?> BuildGenericError(IHostEnvironment env, Exception? ex)
        => env.IsDevelopment()
           ? ApiResponse.BadRequest(ex?.Message ?? "Unhandled error",
                 new[] { new ApiError("Exception", ex?.ToString() ?? string.Empty) })
           : ApiResponse.BadRequest("An unexpected error occurred. " +
                                    "Please contact support.");
}
