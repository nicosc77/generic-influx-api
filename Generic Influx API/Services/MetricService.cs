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
        _measurement = configuration["Influx:Measurement"];
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
            .Tag(ProjectTag, metric.Project)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
        metric.DataFeatures!.ToList().ForEach(entry =>
        {
            builder = builder
                .Field($"{entry.Key}{TypeKey}", entry.Value.Type)
                .Field($"{entry.Key}{ValueKey}", entry.Value.Value);
        });
        await _writeApi.WritePointAsync(builder, _bucket, _org);
    }

    public async Task<List<GenericMetric>> List(string? location, string? project)
    {
        var flux = new System.Text.StringBuilder();
        flux.Append($"from(bucket:\"{_bucket}\")\n |> range(start: 0) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")");
        if (location is not null) flux.Append($"|> filter(fn:(r) => r.location == \"{location}\")");
        if (project is not null) flux.Append($"|> filter(fn:(r) => r.project == \"{project}\")");
        var list = new List<GenericMetric>();
        await _queryApi.QueryAsync(flux.ToString(), record =>
        {
            var result = new GenericMetric
            {
                Timestamp = (DateTime) record.GetTimeInDateTime()!
            };
            if (record.Values.ContainsKey(LocationTag)) result.Location = record.Values[LocationTag].ToString();
            if (record.Values.ContainsKey(ProjectTag)) result.Project = record.Values[ProjectTag].ToString();
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
                            Value = entry.Value
                        };
                    else
                        result.DataFeatures![fieldName].Value = entry.Value;
                }
            });
            list.Add(result);
        }, error => throw new Exception(error.Message), () => { }, _org);
        return list;
    }

    public async Task<GenericMetric> GetFirst(string? location, string? project)
    {
        var flux = new System.Text.StringBuilder();
        flux.Append($"from(bucket:\"{_bucket}\")\n |> range(start: 0) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")");
        if (location is not null) flux.Append($"|> filter(fn:(r) => r.location == \"{location}\")");
        if (project is not null) flux.Append($"|> filter(fn:(r) => r.project == \"{project}\")");
        var list = new List<GenericMetric>();
        await _queryApi.QueryAsync(flux.ToString(), record =>
        {
            var result = new GenericMetric
            {
                Timestamp = (DateTime) record.GetTimeInDateTime()!
            };
            if (record.Values.ContainsKey(LocationTag)) result.Location = record.Values[LocationTag].ToString();
            if (record.Values.ContainsKey(ProjectTag)) result.Project = record.Values[ProjectTag].ToString();
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
                            Value = entry.Value
                        };
                    else
                        result.DataFeatures![fieldName].Value = entry.Value;
                }
            });
            list.Add(result);
        }, error => throw new Exception(error.Message), () => { }, _org);
        return list[0];
    }
}