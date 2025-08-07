using System.Diagnostics;
using MediatR;
using Serilog.Context;

namespace RESTfulAPI.Application.Behaviors
{
    /// <summary>
    /// A MediatR pipeline behavior that enriches each request with Serilog log context properties.
    /// Pushes the request’s type name as "RequestName" and the current Activity.TraceId as "CorrelationId".
    /// Ensures these properties flow through all log entries produced during handling.
    /// </summary>
    public class SerilogEnrichingBehavior<TReq, TRes> : IPipelineBehavior<TReq, TRes>
        where TReq : notnull
    {
        public async Task<TRes> Handle(
            TReq request,
            RequestHandlerDelegate<TRes> next,
            CancellationToken ct)
        {
            using (LogContext.PushProperty("RequestName", typeof(TReq).Name))
            using (LogContext.PushProperty("CorrelationId",
                   Activity.Current?.TraceId.ToString() ?? string.Empty))
            {
                return await next();
            }
        }
    }
}


