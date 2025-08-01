using FluentValidation;
using MediatR;

public sealed class ValidationBehavior<TReq, TRes>
    : IPipelineBehavior<TReq, TRes> where TReq : notnull
{
    private readonly IEnumerable<IValidator<TReq>> _validators;
    private readonly ILogger<ValidationBehavior<TReq, TRes>> _log;

    public ValidationBehavior(IEnumerable<IValidator<TReq>> validators,
                              ILogger<ValidationBehavior<TReq, TRes>> log)
    {
        _validators = validators;
        _log = log;
    }

    public async Task<TRes> Handle(
        TReq request,
        RequestHandlerDelegate<TRes> next,
        CancellationToken ct)
    {
        if (_validators.Any())
        {
            var ctx = new ValidationContext<TReq>(request);

            var failures = (await Task.WhenAll(
                                _validators.Select(v => v.ValidateAsync(ctx, ct))))
                           .SelectMany(r => r.Errors)
                           .Where(f => f is not null)
                           .ToList();

            if (failures.Count != 0)
            {
                _log.LogWarning("Validation failed for {Request}: {@Failures}",
                                typeof(TReq).Name, failures);
                throw new ValidationException(failures);  // <-- caught by middleware
            }
        }

        return await next();
    }
}
