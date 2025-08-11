using MediatR;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Infrastructure.Repositories.Interfaces;

namespace RESTfulAPI.Application.MediatR.Handlers
{
    public class GetPatientsHandler(IPatientRepository _patientRepository) : IRequestHandler<GetPatientsRequest, IReadOnlyList<Patient>>
    {
        async Task<IReadOnlyList<Patient>> IRequestHandler<GetPatientsRequest, IReadOnlyList<Patient>>.Handle(GetPatientsRequest request, CancellationToken cancellationToken)
        {
            var patients = await _patientRepository.GetAllAsync(cancellationToken);

            return patients;
        }
    }
}
