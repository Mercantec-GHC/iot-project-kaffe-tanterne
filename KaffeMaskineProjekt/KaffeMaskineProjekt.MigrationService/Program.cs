using KaffeMaskineProjekt.MigrationService;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.Repository;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();
builder.Services.AddOpenTelemetry()

    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.AddNpgsqlDbContext<KaffeDBContext>("KaffeDBServer");

var host = builder.Build();
host.Run();
