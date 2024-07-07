using FluentValidation;
using MediatR;

namespace Greenfield.Infrastructure.Behaviors;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Implicitly invoked by MediatR")]
internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;
    private readonly ILogger _logger = logger.ForContext<ValidationBehavior<TRequest, TResponse>>();
    
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (!_validators.Any())
        {
            return await next();
        }
        
        var validationContext = new ValidationContext<TRequest>(request);
        var validationResults =
            await Task.WhenAll(_validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));
        
        var errors = validationResults.Where(v => v.Errors.Count > 0).SelectMany(v => v.Errors).ToArray();
        if (errors.Length != 0)
        {
            _logger.ForContext("Issues", errors)
                .Information("Validation failed for {TRequest}", typeof(TRequest).Name);
            
            throw new ValidationException("One or more validation errors occurred", errors);
        }
        
        return await next();
    }
}