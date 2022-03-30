using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using mqtt_influx_bridge.Models;

namespace mqtt_influx_bridge.Services;

public class MetricService
{
    private readonly InfluxDBClient _influxDbClient;

    public MetricService(IConfiguration configuration)
    {
        var url = configuration["Influx:Url"];
        var token = configuration["Influx:Token"];
        _influxDbClient = InfluxDBClientFactory.Create(url, token);
    }

    public async Task Write(GenericMetric metric)
    {
        var writeApi = _influxDbClient.GetWriteApiAsync();
        var point = PointData.Measurement("GenericMeasurement")
            .Tag("location", "west")
            .Field("value", 55D)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
        await writeApi.WritePointAsync(point, "Metric", "Test");
    }

    public async Task<List<GenericMetric>> List()
    {
        const string flux = "from(bucket:\"Metric\") |> range(start: 0)";
        var fluxTables = await _influxDbClient.GetQueryApi().QueryAsync(flux, "Test");
        var result = new List<GenericMetric>();
        fluxTables.ForEach(fluxTable =>
        {
            var fluxRecords = fluxTable.Records;
            fluxRecords.ForEach(fluxRecord =>
            {
                var features = new Dictionary<string, GenericDataFeature>
                    {{"voltage", new GenericDataFeature(fluxRecord.GetValue().ToString()!)}};
                result.Add(new GenericMetric((DateTime) fluxRecord.GetTimeInDateTime()!, features));
            });
        });
        return result;
    }
}