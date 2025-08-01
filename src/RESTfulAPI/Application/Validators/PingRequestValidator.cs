using FluentValidation;

public class PingRequestValidator : AbstractValidator<PingRequest>
{
    public PingRequestValidator()
    {
        RuleFor(r => r.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(50);
    }
}