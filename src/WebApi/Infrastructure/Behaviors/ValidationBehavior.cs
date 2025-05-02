using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Immediate.Handlers.Shared;

namespace WebApi.Infrastructure.Behaviors;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "ImmediateHandlers require behaviors to be public to be discoverable")]
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger
) : Behavior<TRequest, TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger = logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    /// <inheritdoc />
    public override async ValueTask<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!_validators.Any())
        {
            return await Next(request, cancellationToken);
        }

        var validationContext = new ValidationContext<TRequest>(request);
        var validationResults =
            await Task.WhenAll(_validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        var errors = validationResults.Where(v => v.Errors.Count > 0).SelectMany(v => v.Errors).ToArray();
        if (errors.Length == 0)
        {
            return await Next(request, cancellationToken);
        }

        using (_logger.BeginScope(("Issues", errors)))
        {
            _logger.LogInformation("Validation failed for {TRequest}", typeof(TRequest).FullName);
        }

        throw new ValidationException("One or more validation errors occurred", errors);
    }
}