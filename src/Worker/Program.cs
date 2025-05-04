using Api.Database;
using Api.Infrastructure;
using Api.Infrastructure.Hangfire;
using Api.Infrastructure.Logging;
using Hangfire;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("secrets.json", true);

builder.AddEntityFramework();
builder.Services.AddSerilogInternal("Worker");
builder.Services.AddWebApiServices();

builder.AddHangfireInternal();
builder.Services.AddHangfireServer(options => { options.Queues = HangfireQueue.GetAll(); });

var host = builder.Build();

RecurringJobProvider.ScheduleRecurringJobsForCurrentAssembly(host.Services);

host.Run();