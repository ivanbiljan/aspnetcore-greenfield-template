using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using WebApi.Infrastructure;
using WebApi.Infrastructure.Hangfire;
using WebApi.Infrastructure.Logging;
using WebApi.Infrastructure.Persistence;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using ProblemDetailsMiddleware = WebApi.Infrastructure.Web.ProblemDetailsMiddleware;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.Configure<ApiBehaviorOptions>(
        options =>
        {
            options.SuppressInferBindingSourcesForParameters = true;
        }
    );
    
    builder.Services.Configure<JsonOptions>(
        options =>
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
    builder.Services.AddSwaggerGen();
    
    builder.AddEntityFramework();
    builder.AddHangfireInternal();
    builder.Services.AddSerilogInternal("Api");
    builder.Services.ConfigureSerilogHttpLogging();
    builder.Services.AddApplicationServices();
    
    var app = builder.Build();
    
    app.UseSerilogRequestLogging();
    
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
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    
    app.MapIdentityApi<IdentityUser>();
    
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

namespace WebApi
{
    public class Program;
}