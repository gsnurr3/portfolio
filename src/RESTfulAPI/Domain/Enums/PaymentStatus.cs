namespace RESTfulAPI.Domain.Enums
{
    public enum PaymentStatus : byte
    {
        Pending = 1,
        Completed = 2,
        Denied = 3,
        Refunded = 4
    }
}