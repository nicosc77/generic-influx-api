using System.Text.Json.Serialization;
using InfluxDB.Client.Core;

namespace mqtt_influx_bridge.Models;

[Measurement("GenericMetric")]
public class GenericMetric
{
    public GenericMetric(DateTime timestamp, IDictionary<string, GenericDataFeature>? dataFeatures, string? location)
    {
        Timestamp = timestamp;
        DataFeatures = dataFeatures;
        Location = location;
    }

    public GenericMetric()
    {
    }

    [JsonInclude]
    [Column("time", IsTimestamp = true)]
    public DateTime Timestamp { get; set; }

    [JsonInclude]
    [Column("location", IsTag = true)]
    public string? Location { get; set; }

    [JsonInclude] public IDictionary<string, GenericDataFeature>? DataFeatures { get; set; }
}