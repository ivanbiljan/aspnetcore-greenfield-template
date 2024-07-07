using WebApi.Infrastructure;
using WebApi.Infrastructure.Hangfire;
using WebApi.Infrastructure.Logging;
using WebApi.Infrastructure.Persistence;
using Hangfire;
using HangfireWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddEntityFramework();
builder.Services.AddSerilogInternal("HangfireWorker");
builder.Services.AddApplicationServices();

builder.AddHangfireInternal();
builder.Services.AddHangfireServer(
    options => { options.Queues = HangfireQueue.GetAll(); }
);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();