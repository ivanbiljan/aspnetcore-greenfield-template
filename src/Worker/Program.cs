using Hangfire;
using WebApi.Database;
using WebApi.Infrastructure;
using WebApi.Infrastructure.Hangfire;
using WebApi.Infrastructure.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("secrets.json", optional: true);

builder.AddEntityFramework();
builder.Services.AddSerilogInternal("Worker");
builder.Services.AddWebApiServices();

builder.AddHangfireInternal();
builder.Services.AddHangfireServer(options => { options.Queues = HangfireQueue.GetAll(); });

var host = builder.Build();

RecurringJobProvider.ScheduleRecurringJobsForCurrentAssembly(host.Services);

host.Run();