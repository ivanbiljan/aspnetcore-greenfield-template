using EntityFramework.Exceptions.Common;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.WebUtilities;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Api.Infrastructure.Web;

internal static class ProblemDetailsMiddleware
{
    public static void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        options.Map<ValidationException>((ctx, ex) =>
            {
                var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                var errors = ex.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(x => x.ErrorMessage).ToArray()
                    );

                return factory.CreateValidationProblemDetails(ctx, errors);
            }
        );

        options.Map<UnauthorizedException>((ctx, ex) =>
            {
                var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                return factory.CreateProblemDetails(
                    ctx,
                    StatusCodes.Status401Unauthorized,
                    ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized),
                    $"https://httpstatuses.io/{StatusCodes.Status401Unauthorized}",
                    ex.Message
                );
            }
        );

        options.Map<ForbiddenException>((ctx, ex) =>
            {
                var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                return factory.CreateProblemDetails(
                    ctx,
                    StatusCodes.Status403Forbidden,
                    ReasonPhrases.GetReasonPhrase(StatusCodes.Status403Forbidden),
                    $"https://httpstatuses.io/{StatusCodes.Status403Forbidden}",
                    ex.Message
                );
            }
        );

        options.Map<NotFoundException>((ctx, ex) =>
            {
                var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                return factory.CreateProblemDetails(
                    ctx,
                    StatusCodes.Status404NotFound,
                    ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound),
                    $"https://httpstatuses.io/{StatusCodes.Status404NotFound}",
                    ex.Message
                );
            }
        );

        options.Map<WebApiException>((ctx, ex) =>
            {
                var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                return factory.CreateProblemDetails(
                    ctx,
                    ex.StatusCode,
                    ReasonPhrases.GetReasonPhrase(ex.StatusCode),
                    $"https://httpstatuses.io/{ex.StatusCode}",
                    ex.Message
                );
            }
        );

        options.MapToStatusCode<UniqueConstraintException>(StatusCodes.Status409Conflict);
        options.MapToStatusCode<InvalidOperationException>(StatusCodes.Status422UnprocessableEntity);
        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

        // Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
        // If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }
}