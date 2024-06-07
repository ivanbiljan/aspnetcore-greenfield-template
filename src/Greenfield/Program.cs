using System.Diagnostics;
using Greenfield.Infrastructure;
using Greenfield.Infrastructure.Hangfire;
using Greenfield.Infrastructure.Logging;
using Greenfield.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddEntityFramework();
    builder.AddHangfireInternal();
    builder.Services.AddSerilogInternal("Api");
    builder.Services.ConfigureSerilogHttpLogging();
    builder.Services.AddApplicationServices();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.Run();
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
    }
    
    Log.CloseAndFlush();
}