using MediatR;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.MediatR.Handlers
{
    public class PingHandler : IRequestHandler<PingRequest, PingResponse>
    {
        async Task<PingResponse> IRequestHandler<PingRequest, PingResponse>.Handle(PingRequest request, CancellationToken cancellationToken)
        {
            var response = new PingResponse();

            response.MessageOne = request.MessageOne.ToUpperInvariant();
            response.MessageTwo = request.MessageTwo.ToLowerInvariant();
            response.MessageThree = request.MessageThree;

            return await Task.FromResult(response);
        }
    }
}
