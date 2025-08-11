using MediatR;
using RESTfulAPI.Domain.Entities;

namespace RESTfulAPI.Application.Requests
{
    public sealed record GetPatientsRequest : IRequest<IReadOnlyList<Patient>>;
}
