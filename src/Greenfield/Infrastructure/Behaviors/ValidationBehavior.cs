using FluentValidation;
using MediatR;

namespace Greenfield.Infrastructure.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger = logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;
    
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
            var additionalLogProperties = new Dictionary<string, object>
            {
                ["Issues"] = errors
            };
            
            using (_logger.BeginScope(additionalLogProperties))
            {
                _logger.LogInformation("Validation failed for {TRequest}", typeof(TRequest).Name);
            }
            
            throw new ValidationException("One or more validation errors occurred", errors);
        }
        
        return await next();
    }
}