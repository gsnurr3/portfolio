using AutoMapper;
using MediatR;
using RESTfulAPI.Application.DTOs;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Infrastructure.Repositories.Interfaces;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.MediatR.Handlers
{
    public class GetPatientsHandler(IMapper _mapper, IPatientRepository _patientRepository)
        : IRequestHandler<GetPatientsRequest, GetPatientsResponse>
    {
        async Task<GetPatientsResponse> IRequestHandler<GetPatientsRequest, GetPatientsResponse>.Handle(GetPatientsRequest request, CancellationToken cancellationToken)
        {
            var response = new GetPatientsResponse();

            response.Patients = _mapper.Map<List<GetPatientsDTO>>(await _patientRepository.GetAllAsync(cancellationToken));

            return response;
        }
    }
}
