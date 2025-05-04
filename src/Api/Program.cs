using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Api.Infrastructure;
using Api.Infrastructure.Behaviors;
using Api.Infrastructure.Hangfire;
using Api.Infrastructure.Logging;
using Api.Infrastructure.Web;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using ProblemDetailsMiddleware = Api.Infrastructure.Web.ProblemDetailsMiddleware;

[assembly: InternalsVisibleTo("Worker")]
[assembly: InternalsVisibleTo("Api.Tests")]
[assembly: Behaviors(typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>))]

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile("secrets.json", true);

    builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressInferBindingSourcesForParameters = true;
        }
    );

    builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }
    );

    builder.Services.AddControllers();

    builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

    builder.Services.AddProblemDetails(ProblemDetailsMiddleware.ConfigureProblemDetails);
    builder.Services.AddProblemDetailsConventions();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwagger();

    builder.AddEntityFramework();
    builder.AddHangfireInternal();
    builder.Services.AddSerilogInternal("Api");
    builder.Services.ConfigureSerilogHttpLogging();
    builder.Services.AddWebApiServices();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseProblemDetails();
    app.UseRouting();
    app.UseMiddleware<RequestIdDecoratorMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapWebApiEndpoints();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Unexpected exception during host bootstrapping");
}
finally
{
    if (new StackTrace().FrameCount == 1)
    {
        Log.Information("Shutdown complete");
        await Log.CloseAndFlushAsync();
    }
}

namespace Api
{
    [SuppressMessage(
        "Maintainability",
        "CA1515:Consider making public types internal",
        Justification = "Required by xUnit"
    )]
    public sealed class Program;
}