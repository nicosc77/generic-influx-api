using System.Text.Json.Serialization;

namespace mqtt_influx_bridge.Models;

public class GenericDataFeature
{
    public GenericDataFeature(string? type, object? value)
    {
        Type = type;
        Value = value;
    }

    public GenericDataFeature()
    {
    }

    [JsonInclude] public string? Type { get; set; }
    [JsonInclude] public object? Value { get; set; }
}