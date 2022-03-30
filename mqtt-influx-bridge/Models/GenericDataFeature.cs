using System.Text.Json.Serialization;

namespace mqtt_influx_bridge.Models;

public class GenericDataFeature
{
    public GenericDataFeature(string type)
    {
        Type = type;
    }

    [JsonInclude] public string Type { get; }
}