using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using mqtt_influx_bridge.Models;
using static mqtt_influx_bridge.Utils.Constants;

namespace mqtt_influx_bridge.Services;

public class MetricService
{
    private readonly string _bucket;
    private readonly string _measurement;
    private readonly string _org;
    private readonly QueryApi _queryApi;
    private readonly WriteApiAsync _writeApi;

    public MetricService(IConfiguration configuration)
    {
        var url = configuration["Influx:Url"];
        var token = configuration["Influx:Token"];
        _org = configuration["Influx:Org"];
        _measurement = configuration["Influx:measurement"];
        _bucket = configuration["Influx:Bucket"];

        var influxDbClient = InfluxDBClientFactory.Create(url, token);
        _writeApi = influxDbClient.GetWriteApiAsync();
        _queryApi = influxDbClient.GetQueryApi();
    }

    public async Task Write(GenericMetric metric)
    {
        var builder = PointData
            .Measurement(_measurement)
            .Tag(LocationTag, metric.Location)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
        metric.DataFeatures!.ToList().ForEach(entry =>
        {
            builder = builder.Field($"{entry.Key}{TypeKey}", entry.Value.Type)
                .Field($"{entry.Key}{ValueKey}", entry.Value);
        });
        await _writeApi.WritePointAsync(builder, _bucket, _org);
    }

    public async Task<List<GenericMetric>> List()
    {
        const string flux = "from(bucket:\"Metric\")\n"
                            + " |> range(start: 0)"
                            + " |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

        var list = new List<GenericMetric>();
        await _queryApi.QueryAsync(flux, record =>
        {
            var result = new GenericMetric();
            result.Timestamp = (DateTime) record.GetTimeInDateTime()!;
            result.DataFeatures = new Dictionary<string, GenericDataFeature>();
            record.Values.ToList().ForEach(entry =>
            {
                var fieldName = entry.Key.Split("_")[0];
                if (entry.Key.EndsWith(TypeKey))
                {
                    if (!result.DataFeatures!.ContainsKey(fieldName))
                        result.DataFeatures![fieldName] = new GenericDataFeature
                        {
                            Type = entry.Value.ToString()
                        };
                    else
                        result.DataFeatures![fieldName].Type = entry.Value.ToString();
                }
                else if (entry.Key.EndsWith(ValueKey))
                {
                    if (!result.DataFeatures!.ContainsKey(fieldName))
                        result.DataFeatures![fieldName] = new GenericDataFeature
                        {
                            Value = entry.Value.ToString()
                        };
                    else
                        result.DataFeatures![fieldName].Value = entry.Value.ToString();
                }
            });
            list.Add(result);
        }, error => throw new Exception(error.Message), () => { }, _org);
        return list;
    }
}