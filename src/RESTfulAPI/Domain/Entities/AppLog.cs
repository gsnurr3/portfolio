namespace RESTfulAPI.Domain.Entities
{
    public class AppLog
    {
        public Guid Id { get; init; }
        public string Message { get; set; } = default!;
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    }
}
