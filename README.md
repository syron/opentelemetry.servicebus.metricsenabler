# Azure Service Bus Metrics Enabler

This service provides metrics to an OTEL endpoint which then can be monitored using e.g. prometheus, grafana, opensearch or other.

Using this service, only metrics of one service bus namespace are being exposed. However, if you need to expose more metrics, you can set up multiple instances.

> Even if the official otel collector supports azure monitor receivers, an Azure Monitor resource is still required. This does not require any Azure Monitor resource.

## Metrics

### Queuese

Per queue, the following metrics are provided

* Active Messages (those are the messages on the queue currently waiting to be picked up)
* Deadletter Messages
* Total Messages

### Topics

Per topic, the following metrics are provided

* Number of consumers/subscribers


## How to run

### Docker

```docker compose up --build```

### Termina/CMD

```dotnet run```