using InfluxDB.Client;
using mqtt_influx_bridge.Models;

namespace mqtt_influx_bridge.Services;

public class MetricService
{
    private static readonly char[] Token = "".ToCharArray();
    private readonly InfluxDBClient _influxDbClient;

    public MetricService(IConfiguration configuration)
    {
        var url = configuration["Influx:Url"];
        var token = configuration["Influx:Token"];
        _influxDbClient = InfluxDBClientFactory.Create(url, token);
    }

    public async Task<List<GenericMetric>> List()
    {
        const string flux = "from(bucket:\"Test\") |> range(start: 0)";
        var fluxTables = await _influxDbClient.GetQueryApi().QueryAsync(flux, "Test");
        var result = new List<GenericMetric>();
        fluxTables.ForEach(fluxTable =>
        {
            var fluxRecords = fluxTable.Records;
            fluxRecords.ForEach(fluxRecord =>
            {
                var features = new Dictionary<string, GenericDataFeature> {{"voltage", new GenericDataFeature("generic.voltage", fluxRecord.GetValue())}};
                result.Add(new GenericMetric((DateTime) fluxRecord.GetTimeInDateTime()!, features));
            });
        });
        return result;
    }
}