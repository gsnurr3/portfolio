using System.Diagnostics;

namespace RESTfulAPI.Presentation.Common
{
    public static class ApiResponseCorrelation
    {
        private static IHttpContextAccessor? _accessor;

        public static void Initialize(IHttpContextAccessor accessor) => _accessor = accessor;

        public static string? Get()
        {
            var ctx = _accessor?.HttpContext;
            if (ctx is null) return Activity.Current?.TraceId.ToString();

            // Prefer the GUID you set in middleware
            if (ctx.Response.Headers.TryGetValue("X-Correlation-ID", out var v) && !string.IsNullOrWhiteSpace(v))
                return v.ToString();

            if (ctx.Items.TryGetValue("CorrelationId", out var o) && o is Guid g)
                return g.ToString();

            return Activity.Current?.TraceId.ToString();
        }
    }
}