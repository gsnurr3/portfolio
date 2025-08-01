using System.Diagnostics;

/// <summary>
/// Standard envelope for everything your API returns.
/// </summary>
public sealed class ApiResponse<TData>
{
    // ── Core ──────────────────────────────────────────────────────────────
    public bool Success { get; }
    public int Status { get; }
    public string? Message { get; }
    public TData? Data { get; }
    public IReadOnlyList<ApiError>? Errors { get; }

    // ── Cross-cutting metadata (diagnostics, tracing, etc.) ───────────────
    public string CorrelationId { get; }
    public DateTimeOffset Timestamp { get; }

    // ── Private constructor forces use of the factories ──────────────────────────
    private ApiResponse(
        bool success,
        int status,
        string? message,
        TData? data,
        IReadOnlyList<ApiError>? errors,
        string? correlationId = null)
    {
        Success = success;
        Status = status;
        Message = message;
        Data = data;
        Errors = errors;
        CorrelationId = correlationId
                      ?? Activity.Current?.TraceId.ToString()   // OpenTelemetry / System.Diagnostics
                      ?? Guid.NewGuid().ToString("N");
        Timestamp = DateTimeOffset.UtcNow;
    }

    // ── Success factories ────────────────────────────────────────────────
    public static ApiResponse<TData> Ok(
        TData data,
        string? message = null,
        string? correlationId = null)
        => new(true, StatusCodes.Status200OK, message, data, null, correlationId);

    public static ApiResponse<TData> Created(
        TData data,
        string location,
        string? message = null,
        string? correlationId = null)
    {
        return new ApiResponse<TData>(
                   true,
                   StatusCodes.Status201Created,
                   message ?? "Resource created.",
                   data,
                   null,
                   correlationId)
               .WithLocation(location);
    }

    public static ApiResponse<TData> NoContent(string? correlationId = null)
        => new(true, StatusCodes.Status204NoContent, null, default, null, correlationId);

    // ── Failure factories ────────────────────────────────────────────────
    public static ApiResponse<TData> BadRequest(
        string message,
        IEnumerable<ApiError>? errors = null,
        string? correlationId = null)
        => new(false, StatusCodes.Status400BadRequest, message,
               default, errors?.ToList(), correlationId);

    public static ApiResponse<TData> Unauthorized(
        string message = "Unauthorized.",
        string? correlationId = null)
        => new(false, StatusCodes.Status401Unauthorized, message,
               default, null, correlationId);

    public static ApiResponse<TData> Forbidden(
        string message = "Forbidden.",
        string? correlationId = null)
        => new(false, StatusCodes.Status403Forbidden, message,
               default, null, correlationId);

    public static ApiResponse<TData> NotFound(
        string message = "Resource not found.",
        string? correlationId = null)
        => new(false, StatusCodes.Status404NotFound, message,
               default, null, correlationId);

    public static ApiResponse<TData> ValidationFailed(
        IEnumerable<ApiError> errors,
        string? correlationId = null)
        => new(false, StatusCodes.Status422UnprocessableEntity,
               "Validation failed.", default, errors.ToList(), correlationId);

    // ── Internal helpers ─────────────────────────────────────────────────
    private ApiResponse<TData> WithLocation(string location)
    {
        // caller adds the HTTP Location header; here we merely stash it
        // Location = location;
        return this;
    }
}

/// <summary>
/// Convenience non-generic façade so callers can write ApiResponse.Ok(...) without a type arg.
/// </summary>
public static class ApiResponse
{
    // ----- Success shortcuts -----
    public static ApiResponse<T> Ok<T>(T data, string? msg = null, string? corr = null)
        => ApiResponse<T>.Ok(data, msg, corr);

    public static ApiResponse<object?> NoContent(string? corr = null)
        => ApiResponse<object?>.NoContent(corr);

    // ----- Failure shortcuts -----
    public static ApiResponse<object?> BadRequest(
        string message, IEnumerable<ApiError>? errs = null, string? corr = null)
        => ApiResponse<object?>.BadRequest(message, errs, corr);

    public static ApiResponse<object?> Unauthorized(
        string? message = null, string? corr = null)
        => ApiResponse<object?>.Unauthorized(message ?? "Unauthorized.", corr);

    public static ApiResponse<object?> Forbidden(
        string? message = null, string? corr = null)
        => ApiResponse<object?>.Forbidden(message ?? "Forbidden.", corr);

    public static ApiResponse<object?> NotFound(
        string? message = null, string? corr = null)
        => ApiResponse<object?>.NotFound(message ?? "Resource not found.", corr);

    public static ApiResponse<object?> ValidationFailed(
        IEnumerable<ApiError> errs, string? corr = null)
        => ApiResponse<object?>.ValidationFailed(errs, corr);
}

/// <summary>
/// Lightweight error detail object.
/// </summary>
public sealed record ApiError(string Code, string Message, string? Target = null);
