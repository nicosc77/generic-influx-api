namespace mqtt_influx_bridge.Models;

public class GenericDataFeature
{
    public GenericDataFeature(string type, object getValue)
    {
        Type = type;
    }

    private string Type { get; }
}