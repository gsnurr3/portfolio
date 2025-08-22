using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Presentation.Common;

namespace RESTfulAPI.Presentation.Middleware;

/// <summary>
/// One gateway for all unhandled exceptions (MVC, MediatR, pipeline).
/// Converts exceptions to ApiResponse envelopes with sensible status codes.
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

                ApiResponse<object?> envelope = ex switch
                {
                    // 422 – FluentValidation
                    ValidationException ve => ApiResponse.ValidationFailed(
                        ve.Errors.Select(e =>
                            new ApiError(e.ErrorCode, e.ErrorMessage, e.PropertyName))),

                    // SQL/EF validation & constraint errors -> 422
                    DbUpdateException dbe => MapDbUpdateException(dbe, env),

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

    // ──────────────────────────────────────────────────────────────────────────
    // EF/SQL mapping - 422 ValidationFailed
    // ──────────────────────────────────────────────────────────────────────────
    private static ApiResponse<object?> MapDbUpdateException(DbUpdateException dbe, IHostEnvironment env)
    {
        // If there’s a SqlException, classify by Number. Otherwise fall back.
        if (dbe.InnerException is SqlException sql)
        {
            var errors = new List<ApiError>();

            // Look at all SqlErrors on the exception to be thorough.
            foreach (SqlError e in sql.Errors)
            {
                switch (e.Number)
                {
                    // Duplicate key (unique/PK)
                    case 2601: // Cannot insert duplicate key row in object with unique index
                    case 2627: // Violation of PRIMARY KEY or UNIQUE KEY constraint
                        {
                            var value = Match(sql.Message, @"duplicate key value is \((?<v>.+?)\)", "v");
                            var column = GuessColumnFromMessage(sql.Message)
                                         ?? "Unique field";

                            var msg = env.IsDevelopment()
                                      ? $"Duplicate value for {column}{(value is null ? "" : $" ('{value}')")}."
                                      : "A record with the same unique value already exists.";

                            errors.Add(new ApiError("Duplicate", msg, column));
                            break;
                        }

                    // NOT NULL violation
                    case 515: // Cannot insert the value NULL into column 'X'
                        {
                            var column = Match(sql.Message, @"column\s+'(?<c>[^']+)'", "c") ?? "Field";
                            var msg = env.IsDevelopment()
                                       ? $"Column '{column}' does not allow NULL values."
                                       : "A required field is missing.";
                            errors.Add(new ApiError("Required", msg, column));
                            break;
                        }

                    // String or binary data would be truncated
                    case 2628:
                        {
                            var column = Match(sql.Message, @"column\s+'(?<c>[^']+)'", "c") ?? "Field";
                            var msg = env.IsDevelopment()
                                       ? $"Value is too long for column '{column}'."
                                       : "One or more values are too long.";
                            errors.Add(new ApiError("MaxLength", msg, column));
                            break;
                        }

                    // FK/CHECK constraint
                    case 547: // Conflicted with the FOREIGN KEY constraint / CHECK constraint
                        {
                            var isFk = sql.Message.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase);
                            if (isFk)
                            {
                                var table = Match(sql.Message, @"table\s+""(?<t>[^""]+)""", "t");
                                var column = Match(sql.Message, @"column\s+'(?<c>[^']+)'", "c");
                                var msg = env.IsDevelopment()
                                           ? $"Invalid reference: {table ?? "related table"}{(column is null ? "" : $". Column '{column}'")}."
                                           : "A related resource referenced by this request does not exist.";
                                errors.Add(new ApiError("ForeignKey", msg, column ?? "ForeignKey"));
                            }
                            else
                            {
                                var constraint = Match(sql.Message, @"CHECK constraint\s+'(?<k>[^']+)'", "k") ?? "Check";
                                var msg = env.IsDevelopment()
                                               ? $"Value violates check constraint '{constraint}'."
                                               : "One or more values violate a data rule.";
                                errors.Add(new ApiError("Constraint", msg));
                            }
                            break;
                        }

                    // Deadlock / lock timeout – not validation, but nicer wording
                    case 1205: // deadlock victim
                    case 1222: // lock request timeout
                        {
                            var msg = env.IsDevelopment()
                                    ? $"Database concurrency issue (SQL {e.Number})."
                                    : "Temporary database contention. Please retry.";
                            errors.Add(new ApiError("Concurrency", msg));
                            break;
                        }

                    default:
                        // Unknown SQL error – don’t spill details in production
                        if (env.IsDevelopment())
                            errors.Add(new ApiError($"Sql{e.Number}", e.Message));
                        break;
                }
            }

            if (errors.Count > 0)
                return ApiResponse.ValidationFailed(errors);
        }

        // Fallback: if here, treat as generic server error (keep old behavior)
        return BuildGenericError(env, dbe);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 500 payloads (detailed in Dev, safe in Prod)
    // ──────────────────────────────────────────────────────────────────────────
    private static ApiResponse<object?> BuildGenericError(IHostEnvironment env, Exception? ex)
        => env.IsDevelopment()
           ? ApiResponse.Error(
                 "Unhandled exception.",
                 new[] { new ApiError("Exception", ex?.ToString() ?? string.Empty) })
           : ApiResponse.Error("An unexpected error occurred. Please contact support.");

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────
    private static string? Match(string input, string pattern, string groupName)
    {
        var m = Regex.Match(input ?? "", pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return m.Success ? m.Groups[groupName].Value : null;
    }

    // Heuristic: try to guess a column from common SQL Server messages
    private static string? GuessColumnFromMessage(string message)
    {
        // Column 'X'
        var col = Match(message, @"column\s+'(?<c>[^']+)'", "c");
        if (!string.IsNullOrEmpty(col)) return col;

        // For schema, if we see Patients + duplicate value that starts with MRN, assume MedicalRecordNumber
        if (message.Contains("dbo.Patients", StringComparison.OrdinalIgnoreCase) &&
            message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase) &&
            message.Contains("MRN", StringComparison.OrdinalIgnoreCase))
        {
            return "MedicalRecordNumber";
        }

        return null;
    }
}
