using MediatR;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.Requests
{
    public sealed record GetPatientsRequest : IRequest<GetPatientsResponse>;
}
