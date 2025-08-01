using MediatR;

public class PingRequest : IRequest<PingResponse>
{
    public string Message { get; set; } = string.Empty;
}