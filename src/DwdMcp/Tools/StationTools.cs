using System.ComponentModel;
using System.Text.Json;
using DwdMcp.Services;
using ModelContextProtocol.Server;

namespace DwdMcp.Tools;

[McpServerToolType]
public sealed class StationTools
{
    private readonly IDwdApiClient _apiClient;

    public StationTools(IDwdApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [McpServerTool(Name = "GetStationOverview"),
     Description("Get weather data (forecasts, daily summaries, and warnings) for one or more DWD weather stations. " +
                 "Returns forecast data including temperature, wind, precipitation, and icons. " +
                 "Station IDs are the official DWD 'Stationskennungen' (e.g. '10865' for Frankfurt, 'G005' for a specific station). " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetStationOverview(
        [Description("One or more DWD station IDs (Stationskennung), comma-separated. " +
                     "Examples: '10865' (single station), '10865,G005' (multiple stations). " +
                     "The list of station IDs can be found at https://www.dwd.de/DE/leistungen/klimadatendeutschland/stationsliste.html")]
        string stationIds,
        CancellationToken cancellationToken)
    {
        var ids = stationIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (ids.Length == 0)
            return JsonSerializer.Serialize(new { error = "No station IDs provided. Please provide at least one DWD station ID." });

        try
        {
            var result = await _apiClient.GetStationOverviewAsync(ids, cancellationToken);
            return JsonSerializer.Serialize(result.Stations, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }

    [McpServerTool(Name = "GetCrowdWeatherReports"),
     Description("Get current crowd-sourced weather reports (Crowdwettermeldungen) submitted by DWD app users. " +
                 "Returns all current reports including location, category, and severity. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetCrowdWeatherReports(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetCrowdWeatherReportsAsync(cancellationToken);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }
}
