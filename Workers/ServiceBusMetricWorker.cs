using System.Diagnostics.Metrics;
using Azure.Messaging.ServiceBus.Administration;

namespace servicebusmetricsenabler.Workers;

public class ServiceBusMetricWorker : BackgroundService
{
    private readonly Meter _meter;

    private readonly ServiceBusMetricsService _metricsService;
    private readonly ILogger<ServiceBusMetricWorker> _logger;
    private readonly ServiceBusAdministrationClient _adminClient;

    public ServiceBusMetricWorker(ILogger<ServiceBusMetricWorker> logger, IMeterFactory meterFactory, ServiceBusMetricsService serviceBusMetricsService) 
    {
        _meter = meterFactory.Create(GLOBALS.SERVICEBUSMETERNAME);
        _metricsService = serviceBusMetricsService;
        _logger = logger;

        string? connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        if (connectionString is null)
            throw new ArgumentNullException("ServiceBus connection string is not provided. Please provide key ServiceBusConnectionString as an environment variable.");

        _adminClient = new ServiceBusAdministrationClient(connectionString);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var dict = new Dictionary<string, object>();

        var queues = _adminClient.GetQueuesAsync(stoppingToken);

        await foreach (var queue in queues)
        {
            var queueProperties = await _adminClient.GetQueueRuntimePropertiesAsync(queue.Name, stoppingToken);
            
            if (queueProperties is not null) {
                var queueDetails = queueProperties.Value;

                _meter.CreateObservableGauge<long>($"queue.{queue.Name}.messages", () => {
                    return
                    [
                        // pretend these measurements were read from a real queue somewhere
                        new Measurement<long>(queueDetails.ActiveMessageCount, new KeyValuePair<string,object?>("count", "active")),
                        new Measurement<long>(queueDetails.DeadLetterMessageCount, new KeyValuePair<string,object?>("count", "deadletter")),
                        new Measurement<long>(queueDetails.TotalMessageCount, new KeyValuePair<string,object?>("count", "total")),
                    ];
                });            

                dict.Add($"queue.{queue.Name}.count.activemessages", queueDetails.ActiveMessageCount);
                dict.Add($"queue.{queue.Name}.count.deadlettermessages", queueDetails.DeadLetterMessageCount);
                dict.Add($"queue.{queue.Name}.count.totalmessages", queueDetails.TotalMessageCount);
            }
        }

        var topics = _adminClient.GetTopicsAsync();
        await foreach (var topic in topics) 
        {
            var topicDetails = (await _adminClient.GetTopicRuntimePropertiesAsync(topic.Name, stoppingToken)).Value;

            var totalmessagecount = _meter.CreateObservableGauge<long>($"topic.{topic.Name}.numberofsubscribers", () => topicDetails.SubscriptionCount, "count", "Number of active messages");

            dict.Add($"topic.{topic.Name}.numberofsubscribers", topicDetails.SubscriptionCount);
        }

        _metricsService.Set(dict);

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
    }
}