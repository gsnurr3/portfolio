using FluentValidation;

public class PingRequestValidator : AbstractValidator<PingRequest>
{
    public PingRequestValidator()
    {
        RuleFor(r => r)
            .NotNull().WithMessage("A non-empty request body is required.");

        RuleFor(r => r.MessageOne)
            .NotEmpty().WithMessage("MessageOne is required.")
            .MaximumLength(50);

        RuleFor(r => r.MessageTwo)
            .NotEmpty().WithMessage("MessageTwo is required.")
            .MaximumLength(50);

        RuleFor(r => r.MessageThree)
            .NotEmpty().WithMessage("MessageThree is required.")
            .MaximumLength(50);
    }
}