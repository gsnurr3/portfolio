using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Serilog.Context;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Persistence;

namespace RESTfulAPI.Presentation.Middleware;

public sealed class RequestLogMiddleware
{
    private readonly RequestDelegate _next;
    private const int MaxBodyChars = 8_000;

    public RequestLogMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx, LogDbContext logDb, IHostEnvironment env)
    {
        var nowUtc = DateTime.UtcNow;
        var sw = Stopwatch.StartNew();

        // IDs (always set/echo)
        var requestId = Guid.NewGuid();
        var correlationId = GetOrCreateCorrelationId(ctx);
        ctx.Response.Headers["X-Request-ID"] = requestId.ToString();
        ctx.Items["CorrelationId"] = correlationId;

        // Capture small JSON request body without consuming it
        string? requestBody = null;
        int? bytesReceived = null;
        var req = ctx.Request;

        if (!string.IsNullOrEmpty(req.ContentType) &&
            req.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            req.EnableBuffering();
            using var reader = new StreamReader(req.Body, Encoding.UTF8, false, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            if (requestBody.Length > MaxBodyChars) requestBody = requestBody[..MaxBodyChars];
            req.Body.Position = 0;
        }

        if (req.ContentLength is long cl && cl <= int.MaxValue)
            bytesReceived = (int)cl;

        // Push identifiers into Serilog so ANY logs (incl. exception handler) carry them
        using (LogContext.PushProperty("AppRequestId", requestId))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString() ?? ctx.TraceIdentifier))
        using (LogContext.PushProperty("Path", req.Path.Value))
        {
            await _next(ctx);
        }

        sw.Stop();

        // After pipeline completes (exception already handled)
        var res = ctx.Response;
        var handledEx = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        var log = new RequestLog
        {
            RequestId = requestId,
            CorrelationId = correlationId,
            UserId = GetUserId(ctx.User),
            RequestTime = nowUtc,
            RequestDate = DateOnly.FromDateTime(nowUtc),
            Method = req.Method,
            Scheme = Trunc(req.Scheme, 5, "http"),
            Host = Trunc(req.Host.ToString(), 255, ""),
            Path = Trunc(req.Path.HasValue ? req.Path.Value! : string.Empty, 2048, ""),
            QueryString = req.QueryString.HasValue ? Trunc(req.QueryString.Value, 4000, "") : null,
            StatusCode = (short)res.StatusCode, // final code from exception handler
            DurationMs = checked((int)sw.Elapsed.TotalMilliseconds),
            RemoteIp = TruncOrNull(ctx.Connection.RemoteIpAddress?.ToString(), 45),
            UserAgent = TruncOrNull(GetHeader(req.Headers, "User-Agent"), 512),
            Referrer = TruncOrNull(GetHeader(req.Headers, "Referer"), 512),
            RequestContentType = TruncOrNull(req.ContentType, 100),
            ResponseContentType = TruncOrNull(res.ContentType, 100),
            BytesReceived = bytesReceived,
            BytesSent = GetBytesSent(res),
            RequestHeaders = SerializeHeaders(req.Headers),
            ResponseHeaders = SerializeHeaders(res.Headers),
            RequestBody = requestBody,
            ResponseBody = null,
            ExceptionType = handledEx?.GetType().FullName,
            ExceptionMessage = handledEx?.Message,
            ExceptionStackTrace = handledEx?.ToString(),
            ServerName = Trunc(Environment.MachineName, 128, "unknown"),
            Environment = Trunc(env.EnvironmentName, 32, "unknown")
        };

        try
        {
            await logDb.AddAsync(log, ctx.RequestAborted);
            await logDb.SaveChangesAsync(ctx.RequestAborted);
        }
        catch (Exception e)
        {
            var lg = ctx.RequestServices.GetRequiredService<ILogger<RequestLogMiddleware>>();
            lg.LogWarning(e, "Failed to persist RequestLog (non-blocking).");
        }
    }

    // Always return a GUID correlation id (header wins; otherwise generate & echo)
    private static Guid GetOrCreateCorrelationId(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("X-Correlation-ID", out var h) &&
            Guid.TryParse(h.ToString(), out var cid))
        {
            ctx.Response.Headers["X-Correlation-ID"] = cid.ToString();
            return cid;
        }
        var cidNew = Guid.NewGuid();
        ctx.Response.Headers["X-Correlation-ID"] = cidNew.ToString();
        return cidNew;
    }

    private static string? GetUserId(ClaimsPrincipal user)
    {
        var oid = user.FindFirstValue("oid");
        if (!string.IsNullOrWhiteSpace(oid)) return oid;
        var sub = user.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(sub)) return sub;
        var sid = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrWhiteSpace(sid) ? null : sid;
    }

    private static int? GetBytesSent(HttpResponse res)
        => (res.ContentLength is long cl && cl <= int.MaxValue) ? (int)cl : null;

    private static string Trunc(string? s, int max, string fallback)
        => string.IsNullOrEmpty(s) ? fallback : (s.Length <= max ? s : s[..max]);

    private static string? TruncOrNull(string? s, int max)
        => string.IsNullOrEmpty(s) ? null : (s.Length <= max ? s : s[..max]);

    private static string? GetHeader(IHeaderDictionary headers, string name)
        => headers.TryGetValue(name, out var v) ? v.ToString() : null;

    private static string SerializeHeaders(IHeaderDictionary headers)
    {
        var redacted = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in headers)
        {
            var key = kvp.Key;
            var val = kvp.Value.ToString(); // safe for default(StringValues)
            if (key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Cookie", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
            {
                redacted[key] = "***REDACTED***";
            }
            else
            {
                redacted[key] = val.Length <= 2000 ? val : val[..2000];
            }
        }
        return JsonSerializer.Serialize(redacted);
    }
}
