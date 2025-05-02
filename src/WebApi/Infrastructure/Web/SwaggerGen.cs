using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Infrastructure.Web;

internal static class SwaggerGen
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(
            options =>
            {
                options.OperationFilter<AuthorizationDescriptionOperationFilter>();

                options.CustomSchemaIds(t =>
                    $"{t.DeclaringType!.Namespace!.Split('.')[^1]}.{t.DeclaringType.Name}.{t.Name}"
                );

                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        In = ParameterLocation.Header,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        Description = "JWT Authorization header using the Bearer scheme."
                    }
                );

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                In = ParameterLocation.Header,
                            },
                            new List<string>()
                        }
                    }
                );

                options.TagActionsBy(
                    api =>
                    {
                        var routeTemplate = api.ActionDescriptor.AttributeRouteInfo?.Template ??
                                            api.ActionDescriptor.EndpointMetadata.OfType<IRouteDiagnosticsMetadata>()
                                                .SingleOrDefault()
                                                ?.Route ?? throw new InvalidOperationException(
                                                "Unable to determine tag for endpoint."
                                            );

                        var splits = routeTemplate["api/".Length..].Split('/', StringSplitOptions.RemoveEmptyEntries);
                        if (splits is not [{ } tag, ..]
                            || string.IsNullOrWhiteSpace(tag))
                        {
                            throw new InvalidOperationException("Unable to determine tag for endpoint.");
                        }

                        return [tag[..1].ToUpperInvariant() + tag[1..]];
                    }
                );
            }
        );

        return services;
    }
}

internal sealed class AuthorizationDescriptionOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute))
        {
            return;
        }

        var authorizeAttributes = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>();

        var summaryBuilder = new StringBuilder(" (Authorized");

        BuildPolicies(authorizeAttributes, summaryBuilder);

        operation.Summary += summaryBuilder.ToString().TrimEnd(';') + ")";
    }

    private static void BuildPolicies(IEnumerable<AuthorizeAttribute> authorizeAttributes, StringBuilder stringBuilder)
    {
        var policies = authorizeAttributes
            .Where(a => !string.IsNullOrWhiteSpace(a.Policy))
            .Select(a => a.Policy)
            .ToList();

        if (policies.Count == 0)
        {
            return;
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"; policies: {string.Join(",", policies)};");
    }
}