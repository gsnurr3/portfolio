namespace RESTfulAPI.Presentation.Middleware
{
    // DI helper
    public static class RequestLogExtensions
    {
        public static IApplicationBuilder UseRequestLogMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<RequestLogMiddleware>();
    }
}
