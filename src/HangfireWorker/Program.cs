using Greenfield.Infrastructure;
using Greenfield.Infrastructure.Hangfire;
using Greenfield.Infrastructure.Logging;
using Greenfield.Infrastructure.Persistence;
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