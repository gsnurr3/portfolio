using FluentValidation;
using RESTfulAPI.Application.Requests;

namespace RESTfulAPI.Application.Validators
{
    public sealed class PostPatientRequestValidator : AbstractValidator<PostPatientRequest>
    {
        // Each token (split by space, hyphen, or apostrophe) must be Title Case:
        // "Jane", "Mary-Jane", "O'Neil" are valid; "jane", "JANE", "McDonald" are not.
        private const string NamePattern = @"^(?:[A-Z][a-z]*)(?:[ '-][A-Z][a-z]*)*$";

        public PostPatientRequestValidator()
        {
            RuleFor(x => x.MedicalRecordNumber)
                .NotEmpty().WithMessage("MedicalRecordNumber is required.")
                .MaximumLength(20)
                .Matches(@"^MRN\d{6}$").WithMessage("MedicalRecordNumber must match pattern MRN000001.")
                .Must(v => !string.Equals(v, "string", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Please provide a real MedicalRecordNumber.");

            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("FirstName is required.")
                .MaximumLength(50)
                .Must(v => v == v.Trim()).WithMessage("FirstName must not start or end with spaces.")
                .Matches(NamePattern).WithMessage(
                    "FirstName must be capitalized: first letter uppercase, remaining letters lowercase. " +
                    "Each name part after a space, hyphen, or apostrophe must also be capitalized.")
                .Must(v => !string.Equals(v, "string", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Please provide a real First Name.");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("LastName is required.")
                .MaximumLength(50)
                .Must(v => v == v.Trim()).WithMessage("LastName must not start or end with spaces.")
                .Matches(NamePattern).WithMessage(
                    "LastName must be capitalized: first letter uppercase, remaining letters lowercase. " +
                    "Each name part after a space, hyphen, or apostrophe must also be capitalized.")
                .Must(v => !string.Equals(v, "string", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Please provide a real Last Name.");

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