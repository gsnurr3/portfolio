using MediatR;
using RESTfulAPI.Domain.Enums;
using RESTfulAPI.Presentation.Responses;

namespace RESTfulAPI.Application.Requests
{
    public sealed class PostPatientRequest : IRequest<PostPatientResponse>
    {
        public string MedicalRecordNumber { get; init; } = default!;
        public string FirstName { get; init; } = default!;
        public string LastName { get; init; } = default!;
        public DateOnly DateOfBirth { get; init; }
        public Gender Gender { get; init; }
        public string? Address { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public int? InsuranceProviderId { get; init; }
        public string? InsurancePolicyNumber { get; init; }
    }
}