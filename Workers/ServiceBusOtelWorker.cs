using Quartz;
using System.Diagnostics.Metrics;
using Azure.Messaging.ServiceBus.Administration;

public class ServiceBusOtelWorker : IJob
{
    private readonly Meter _meter;

    private readonly ServiceBusMetricsService _metricsService;
    private readonly ILogger<ServiceBusOtelWorker> _logger;
    private readonly ServiceBusAdministrationClient _adminClient;

    public ServiceBusOtelWorker(ILogger<ServiceBusOtelWorker> logger, IMeterFactory meterFactory, ServiceBusMetricsService serviceBusMetricsService)
    {
        _meter = meterFactory.Create(GLOBALS.SERVICEBUSMETERNAME);
        _metricsService = serviceBusMetricsService;
        _logger = logger;

        string? connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        if (connectionString is null)
            throw new ArgumentNullException("ServiceBus connection string is not provided. Please provide key ServiceBusConnectionString as an environment variable.");

        _adminClient = new ServiceBusAdministrationClient(connectionString);
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Started ServiceBusOtelWorker worker");

        var dict = new Dictionary<string, object>();

        var queues = _adminClient.GetQueuesAsync(context.CancellationToken);

        await foreach (var queue in queues)
        {
            var queueProperties = await _adminClient.GetQueueRuntimePropertiesAsync(queue.Name, context.CancellationToken);
            
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

        var topics = _adminClient.GetTopicsAsync(context.CancellationToken);
        await foreach (var topic in topics) 
        {
            var topicDetails = (await _adminClient.GetTopicRuntimePropertiesAsync(topic.Name, context.CancellationToken)).Value;

            var totalmessagecount = _meter.CreateObservableGauge<long>($"topic.{topic.Name}.numberofsubscribers", () => topicDetails.SubscriptionCount, "count", "Number of active messages");

            dict.Add($"topic.{topic.Name}.numberofsubscribers", topicDetails.SubscriptionCount);
        }

        _metricsService.Set(dict);
    }
}       