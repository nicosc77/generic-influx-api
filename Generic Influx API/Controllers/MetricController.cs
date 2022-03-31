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

    [HttpGet(Name = "ListGenericMetric", Order = 1)]
    [Route("/[controller]/", Order = 1)]
    public async Task<List<GenericMetric>> List(
        [FromQuery(Name = "location")] string? location = null,
        [FromQuery(Name = "project")] string? project = null)
    {
        _logger.LogInformation("Fetching array entities");
        return await _metricService.List(location, project);
    }

    [HttpGet(Name = "GetGenericMetric", Order = 2)]
    [Route("/[controller]/first/", Order = 2)]
    public async Task<GenericMetric> GetFirst(
        [FromQuery(Name = "location")] string? location = null,
        [FromQuery(Name = "project")] string? project = null)
    {
        _logger.LogInformation("Fetching first entity");
        return await _metricService.GetFirst(location, project);
    }

    [HttpPost(Name = "WriteGenericMetric")]
    public async Task<IActionResult> Write(GenericMetric metric)
    {
        _logger.LogInformation("Writing data point");
        await _metricService.Write(metric);
        return Ok();
    }
}