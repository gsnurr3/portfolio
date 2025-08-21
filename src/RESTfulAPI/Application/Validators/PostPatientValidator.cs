using FluentValidation;
using RESTfulAPI.Application.Requests;

namespace RESTfulAPI.Application.Validators
{
    public sealed class PostPatientRequestValidator : AbstractValidator<PostPatientRequest>
    {
        public PostPatientRequestValidator()
        {
            RuleFor(x => x.MedicalRecordNumber)
                .NotEmpty().WithMessage("MedicalRecordNumber is required.")
                .MaximumLength(20)
                .Matches(@"^MRN\d{6}$").WithMessage("MedicalRecordNumber must match pattern MRN000001.")
                .Must(v => !string.Equals(v, "string", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Please provide a real MedicalRecordNumber.");

            RuleFor(x => x.FirstName)
                .NotEmpty().MaximumLength(50)
                .Matches(@"^[A-Za-z][A-Za-z '\-]*$").WithMessage("FirstName contains invalid characters.")
                .Must(v => !string.Equals(v, "string", StringComparison.OrdinalIgnoreCase));

            RuleFor(x => x.LastName)
                .NotEmpty().MaximumLength(50)
                .Matches(@"^[A-Za-z][A-Za-z '\-]*$").WithMessage("LastName contains invalid characters.")
                .Must(v => !string.Equals(v, "string", StringComparison.OrdinalIgnoreCase));

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .GreaterThan(new DateOnly(1900, 1, 1));

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Gender must be Male, Female, or Other.");

            RuleFor(x => x.Address)
                .MaximumLength(200);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .Matches(@"^\+?[0-9()\-\s]{7,20}$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Email)
                .MaximumLength(100)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.InsuranceProviderId)
                .GreaterThan(0)
                .When(x => x.InsuranceProviderId.HasValue);

            RuleFor(x => x.InsurancePolicyNumber)
                .MaximumLength(50)
                .Must(v => !string.Equals(v ?? "", "string", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Please provide a real InsurancePolicyNumber.")
                .When(x => !string.IsNullOrWhiteSpace(x.InsurancePolicyNumber));
        }
    }
}