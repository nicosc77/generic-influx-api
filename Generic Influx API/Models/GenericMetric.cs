using System.Text.Json.Serialization;
using InfluxDB.Client.Core;
using static mqtt_influx_bridge.Utils.Constants;

namespace mqtt_influx_bridge.Models;

[Measurement("GenericMetric")]
public class GenericMetric
{
    public GenericMetric(DateTime timestamp, string? location, string? project,
        IDictionary<string, GenericDataFeature>? dataFeatures)
    {
        Timestamp = timestamp;
        Location = location;
        Project = project;
        DataFeatures = dataFeatures;
    }

    public GenericMetric()
    {
    }

    [JsonInclude]
    [Column(TimeKey, IsTimestamp = true)]
    public DateTime Timestamp { get; set; }

    [JsonInclude]
    [Column(LocationTag, IsTag = true)]
    public string? Location { get; set; }

    [JsonInclude]
    [Column(ProjectTag, IsTag = true)]
    public string? Project { get; set; }

    [JsonInclude] public IDictionary<string, GenericDataFeature>? DataFeatures { get; set; }
}