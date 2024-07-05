# Azure Service Bus Metrics Enabler

This service provides metrics to an OTEL (OpenTelemetry) endpoint which then can be monitored using e.g. prometheus, grafana, opensearch or other.

Using this service, only metrics of one service bus namespace are being exposed. However, if you need to expose more metrics, you can set up multiple instances.

> Even if the official otel collector supports azure monitor receivers, an Azure Monitor resource is still required. This does not require any Azure Monitor resource.

> This project is just meant as an example of how simple it is to accomplish quite advanced tasks using open telemetry.

## Metrics

### Queues

Per queue, the following metrics are provided

* Active Messages (those are the messages on the queue currently waiting to be picked up)
* Deadletter Messages
* Total Messages

### Topics

Per topic, the following metrics are provided

* Number of consumers/subscribers

## Prometheus Metrics Scrape Endpoint

Even though this service is meant to be used as an OTEL service, rather than a Prometheus scraping endpoint, the current version provides a scraping endpoint.

<details>
 <summary><code>GET</code> <code><b>/metrics</b></code> <code>(gets all metrics according to prometheus format)</code></summary>

##### Responses

> | http code     | content-type                      | response                                                            |
> |---------------|-----------------------------------|---------------------------------------------------------------------|
> | `200`         | `text/plain;charset=UTF-8`        | \# TYPE queue_blablabla_messages gauge<br />queue_blablabla_messages{otel_scope_name="ServiceBus",count="active"} 0 1720205607277<br />queue_blablabla_messages{otel_scope_name="ServiceBus",count="deadletter"} 6 1720205607277<br />queue_blablabla_messages{otel_scope_name="ServiceBus",count="total"} 6 1720205607277<br />\# TYPE queue_blub_messages gauge<br />queue_blub_messages{otel_scope_name="ServiceBus",count="active"} 0 1720205607277<br />queue_blub_messages{otel_scope_name="ServiceBus",count="deadletter"} 1 1720205607277<br />queue_blub_messages{otel_scope_name="ServiceBus",count="total"} 1 1720205607277<br />\# TYPE queue_test_messages gauge<br />queue_test_messages{otel_scope_name="ServiceBus",count="active"} 6 1720205607277<br />queue_test_messages{otel_scope_name="ServiceBus",count="deadletter"} 0 1720205607277<br />queue_test_messages{otel_scope_name="ServiceBus",count="total"} 6 1720205607277\#EOF  |

##### Example cURL

> ```javascript
>  curl -X GET -H "Content-Type: application/json" http://localhost:8080/metrics
> ```

</details>

## How to run

### Docker

Make a copy of **compose.example.yaml** and rename it to **compose.yaml**. Also, provide the correct environment variables. Those variables are setting your azure service bus connection and to your open telemetry endpoint.

```docker compose up --build```

### Termina/CMD

In order to run this service, an environment variable for your service bus needs to be set.

Environment variables:

* ServiceBusConnectionString

```dotnet run```