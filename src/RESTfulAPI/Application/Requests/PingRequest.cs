using MediatR;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.Requests
{
    public class PingRequest : IRequest<PingResponse>
    {
        public string MessageOne { get; set; } = string.Empty;
        public string MessageTwo { get; set; } = string.Empty;
        public string MessageThree { get; set; } = string.Empty;
    }
}
