using RESTfulAPI.Domain.Enums;

namespace RESTfulAPI.Application.DTOs
{
    public sealed class GetPatientsDTO
    {
        public Guid PatientId { get; init; }
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
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}