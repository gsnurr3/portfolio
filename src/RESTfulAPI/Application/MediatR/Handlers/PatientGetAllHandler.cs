using MediatR;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Infrastructure.Repositories.Interfaces;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.MediatR.Handlers
{
    public class PatientGetAllHandler(IPatientRepository _patientRepository) : IRequestHandler<PatientGetAllRequest, PatientGetAllResponse>
    {
        async Task<PatientGetAllResponse> IRequestHandler<PatientGetAllRequest, PatientGetAllResponse>.Handle(PatientGetAllRequest request, CancellationToken cancellationToken)
        {
            var patientGetAllResponse = new PatientGetAllResponse();

            patientGetAllResponse.Patients = await _patientRepository.GetAllPatientsAsync();

            return patientGetAllResponse;
        }
    }
}
