using System.Diagnostics;
using MediatR;
using Serilog.Context;

namespace RESTfulAPI.Application.Behaviors
{
    public class SerilogEnrichingBehavior<TReq, TRes> : IPipelineBehavior<TReq, TRes>
        where TReq : notnull
    {
        public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
        {
            using (LogContext.PushProperty("RequestName", typeof(TReq).Name))
            using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString()))
            using (LogContext.PushProperty("SpanId", Activity.Current?.SpanId.ToString()))
            {
                return await next();
            }
        }
    }
}
