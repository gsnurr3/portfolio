using MediatR;

public class PingHandler : IRequestHandler<PingRequest, PingResponse>
{
    async Task<PingResponse> IRequestHandler<PingRequest, PingResponse>.Handle(PingRequest request, CancellationToken cancellationToken)
    {
        var response = new PingResponse();

        response.Message = request.Message.ToUpperInvariant();

        return await Task.FromResult(response);
    }
}