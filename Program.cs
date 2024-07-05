using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using servicebusmetricsenabler.Workers;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "ServiceBusMetricsEnabler";
var serviceVersion = "1.0.0";

builder.Logging.AddOpenTelemetry(logging => {
                                    logging.IncludeFormattedMessage = true;
                                    logging.IncludeScopes = true;
                                    logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                        .AddService(serviceName: serviceName, serviceVersion: serviceVersion, serviceInstanceId: serviceName));
                                });

builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics => {
                    metrics
                        .AddMeter(GLOBALS.SERVICEBUSMETERNAME)
                        .AddPrometheusExporter()
                        .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: serviceName, serviceVersion: serviceVersion, serviceInstanceId: serviceName));
                })
                .UseOtlpExporter();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ServiceBusMetricsService>();

builder.Services.AddHostedService<ServiceBusMetricWorker>();

var app = builder.Build();

app.MapPrometheusScrapingEndpoint("/customservicebusmetrics"); // this can be used if otel endpoint is not available.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
