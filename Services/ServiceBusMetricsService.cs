

using Microsoft.Net.Http.Headers;

public class ServiceBusMetricsService
{
    private Dictionary<string, object> _metrics = new Dictionary<string, object>();

    public void Set(Dictionary<string, object> metrics) 
    {
        _metrics = metrics;
    }

    public Dictionary<string, object> Get()
    {
        return _metrics;
    }
}