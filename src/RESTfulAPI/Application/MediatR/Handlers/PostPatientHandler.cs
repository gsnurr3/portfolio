using AutoMapper;
using MediatR;
using RESTfulAPI.Application.DTOs;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Infrastructure.Repositories.Interfaces;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.MediatR.Handlers
{
    public sealed class PostPatientHandler(IMapper _mapper, IPatientRepository _patientRepository)
        : IRequestHandler<PostPatientRequest, PostPatientResponse>
    {
        public async Task<PostPatientResponse> Handle(PostPatientRequest request, CancellationToken cancellationToken)
        {
            var patientEntity = new Patient
            {
                MedicalRecordNumber = request.MedicalRecordNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                InsuranceProviderId = request.InsuranceProviderId,
                InsurancePolicyNumber = request.InsurancePolicyNumber
            };

            var savedPatient = await _patientRepository.CreateAsync(patientEntity, cancellationToken);

            var postPatientDTO = _mapper.Map<PostPatientDTO>(savedPatient);

            return new PostPatientResponse { Patient = postPatientDTO };
        }
    }
}