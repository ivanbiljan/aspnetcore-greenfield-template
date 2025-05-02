using WebApi.Infrastructure;
using WebApi.Infrastructure.Hangfire;
using WebApi.Infrastructure.Logging;
using Hangfire;
using WebApi.Infrastructure.Database;

var builder = Host.CreateApplicationBuilder(args);

builder.AddEntityFramework();
builder.Services.AddSerilogInternal("Worker");
builder.Services.AddWebApiServices();

builder.AddHangfireInternal();
builder.Services.AddHangfireServer(
    options => { options.Queues = HangfireQueue.GetAll(); }
);

builder.Services.AddHostedService<Worker.Worker>();

var host = builder.Build();
host.Run();