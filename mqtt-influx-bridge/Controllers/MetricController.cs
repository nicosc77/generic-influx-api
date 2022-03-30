using Microsoft.AspNetCore.Mvc;
using mqtt_influx_bridge.Models;
using mqtt_influx_bridge.Services;

namespace mqtt_influx_bridge.Controllers;

[ApiController]
[Route("[controller]")]
public class MetricController : ControllerBase
{
    private readonly ILogger<MetricController> _logger;
    private readonly MetricService _metricService;

    public MetricController(ILogger<MetricController> logger, MetricService metricService)
    {
        _logger = logger;
        _metricService = metricService;
    }

    [HttpGet(Name = "ListGenericMetric")]
    public Task<List<GenericMetric>> List()
    {
        _logger.LogInformation("Fetching array entities");
        var result = _metricService.List();
        return result;
    }

    [HttpPost(Name = "WriteGenericMetric")]
    public async Task<IActionResult> Write(GenericMetric metric)
    {
        _logger.LogInformation("Writing data point");
        await _metricService.Write(metric);
        return Ok();
    }
}