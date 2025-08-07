using MediatR;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Persistence;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.MediatR.Handlers
{
    public class PingHandler(LogDbContext _logContext) : IRequestHandler<PingRequest, PingResponse>
    {
        async Task<PingResponse> IRequestHandler<PingRequest, PingResponse>.Handle(PingRequest request, CancellationToken cancellationToken)
        {
            var response = new PingResponse();

            response.MessageOne = request.MessageOne.ToUpperInvariant();
            response.MessageTwo = request.MessageTwo.ToLowerInvariant();
            response.MessageThree = request.MessageThree;

            _logContext.AppLogs.Add(new AppLog
            {
                Id = new Guid(),
                Message = "This is a test for the interview.",
                Timestamp = DateTimeOffset.UtcNow
            });

            await _logContext.SaveChangesAsync();

            return await Task.FromResult(response);
        }
    }
}
