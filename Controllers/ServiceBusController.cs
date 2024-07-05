
using System.Diagnostics.Metrics;
using System.Security.Cryptography.X509Certificates;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

namespace servicebusmetricsenabler.Controllers;

[ApiController]
[Route("[controller]")]
public class ServiceBusController : ControllerBase
{
    private readonly ILogger<ServiceBusController> _logger;
    private readonly ServiceBusMetricsService _serviceBusMetricsService;

    public ServiceBusController(ILogger<ServiceBusController> logger, ServiceBusMetricsService serviceBusMetricsService)
    {
        _logger = logger;
        _serviceBusMetricsService = serviceBusMetricsService;
    }

    [HttpGet]
    public Dictionary<string, object> Get() 
    {
        return _serviceBusMetricsService.Get();
    }
}