using System.Text.Json.Serialization;

namespace DwdMcp.Models;

public sealed record ForecastData
{
    [JsonPropertyName("stationId")]
    public string? StationId { get; init; }

    [JsonPropertyName("start")]
    public long? Start { get; init; }

    [JsonPropertyName("timeStep")]
    public int? TimeStep { get; init; }

    [JsonPropertyName("temperature")]
    public List<double?>? Temperature { get; init; }

    [JsonPropertyName("temperatureStd")]
    public List<double?>? TemperatureStd { get; init; }

    [JsonPropertyName("windSpeed")]
    public string? WindSpeed { get; init; }

    [JsonPropertyName("windDirection")]
    public string? WindDirection { get; init; }

    [JsonPropertyName("windGust")]
    public string? WindGust { get; init; }

    [JsonPropertyName("icon")]
    public List<int?>? Icon { get; init; }

    [JsonPropertyName("precipitationTotal")]
    public List<double?>? PrecipitationTotal { get; init; }

    [JsonPropertyName("precipitationProbablity")]
    public string? PrecipitationProbablity { get; init; }

    [JsonPropertyName("precipitationProbablityIndex")]
    public string? PrecipitationProbablityIndex { get; init; }
}

public sealed record DayForecast
{
    [JsonPropertyName("stationId")]
    public string? StationId { get; init; }

    [JsonPropertyName("dayDate")]
    public string? DayDate { get; init; }

    [JsonPropertyName("temperatureMin")]
    public int? TemperatureMin { get; init; }

    [JsonPropertyName("temperatureMax")]
    public int? TemperatureMax { get; init; }

    [JsonPropertyName("icon")]
    public int? Icon { get; init; }

    [JsonPropertyName("icon1")]
    public string? Icon1 { get; init; }

    [JsonPropertyName("icon2")]
    public string? Icon2 { get; init; }

    [JsonPropertyName("precipitation")]
    public int? Precipitation { get; init; }

    [JsonPropertyName("windSpeed")]
    public int? WindSpeed { get; init; }

    [JsonPropertyName("windGust")]
    public int? WindGust { get; init; }

    [JsonPropertyName("windDirection")]
    public int? WindDirection { get; init; }

    [JsonPropertyName("sunshine")]
    public int? Sunshine { get; init; }
}

public sealed record StationData
{
    [JsonPropertyName("forecast1")]
    public ForecastData? Forecast1 { get; init; }

    [JsonPropertyName("forecast2")]
    public ForecastData? Forecast2 { get; init; }

    [JsonPropertyName("forecastStart")]
    public string? ForecastStart { get; init; }

    [JsonPropertyName("days")]
    public List<DayForecast>? Days { get; init; }

    [JsonPropertyName("warnings")]
    public List<object>? Warnings { get; init; }

    [JsonPropertyName("threeHourSummaries")]
    public string? ThreeHourSummaries { get; init; }
}

/// <summary>
/// Response from /stationOverviewExtended. Keys are station IDs, values are station data.
/// Deserialized as a dictionary since station IDs are dynamic keys.
/// </summary>
public sealed record StationOverviewResponse
{
    public Dictionary<string, StationData> Stations { get; init; } = new();
}
