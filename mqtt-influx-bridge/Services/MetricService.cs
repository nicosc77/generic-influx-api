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
        var builder = PointData.Measurement("GenericMeasurement").Tag("location", metric.Location)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
        foreach (var (key, value) in metric.DataFeatures!)
            builder = builder.Field($"{key}_type", value.Type).Field($"{key}_value", value.Value);
        await writeApi.WritePointAsync(builder, "Metric", "Test");
    }

    public async Task<List<GenericMetric>> List()
    {
        const string flux =
            "from(bucket:\"Metric\") |> range(start: 0) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
        var queryApi = _influxDbClient.GetQueryApi();
        var list = new List<GenericMetric>();
        await queryApi.QueryAsync(flux, record =>
            {
                var result = new GenericMetric();
                result.Timestamp = (DateTime) record.GetTimeInDateTime()!;
                result.DataFeatures = new Dictionary<string, GenericDataFeature>();
                record.Values.ToList().ForEach(entry =>
                {
                    var fieldName = entry.Key.Split("_")[0];
                    if (entry.Key.EndsWith("_type"))
                    {
                        if (!result.DataFeatures!.ContainsKey(fieldName))
                        {
                            result.DataFeatures![fieldName] = new GenericDataFeature();
                            result.DataFeatures![fieldName].Type = entry.Value.ToString();
                        }
                        else
                        {
                            result.DataFeatures![fieldName].Type = entry.Value.ToString();
                        }
                    }
                    else if (entry.Key.EndsWith("_value"))
                    {
                        if (!result.DataFeatures!.ContainsKey(fieldName))
                        {
                            result.DataFeatures![fieldName] = new GenericDataFeature();
                            result.DataFeatures![fieldName].Value = entry.Value.ToString();
                        }
                        else
                        {
                            result.DataFeatures![fieldName].Value = entry.Value.ToString();
                        }
                    }
                });
                list.Add(result);
            },
            error => { Console.WriteLine(error.ToString()); }, () => { Console.WriteLine("Query completed"); }, "Test");
        return list;
    }
}