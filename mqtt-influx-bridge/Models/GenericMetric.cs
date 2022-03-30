namespace mqtt_influx_bridge.Models;

public class GenericMetric
{
    public GenericMetric(DateTime timestamp, Dictionary<string, GenericDataFeature> dataFeatures)
    {
        Timestamp = timestamp;
        DataFeatures = dataFeatures;
    }

    private DateTime Timestamp { get; }
    private Dictionary<string, GenericDataFeature> DataFeatures { get; }
}