using System.ComponentModel;
using System.Text.Json;
using DwdMcp.Services;
using ModelContextProtocol.Server;

namespace DwdMcp.Tools;

[McpServerToolType]
public sealed class ForecastTools
{
    private readonly IDwdApiClient _apiClient;

    public ForecastTools(IDwdApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [McpServerTool(Name = "GetSeaWarningText"),
     Description("Get current high-sea (Hochsee) weather warnings from DWD as plain text. " +
                 "Returns a text-based warning bulletin for open sea conditions. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetSeaWarningText(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetSeaWarningTextAsync(cancellationToken);
            return result ?? "No sea warning data available.";
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }

    [McpServerTool(Name = "GetAlpineWeatherForecast"),
     Description("Get the current alpine (Alpen) weather forecast from DWD as plain text. " +
                 "Returns a text-based weather forecast for the Alpine region. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetAlpineWeatherForecast(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetAlpineWeatherForecastAsync(cancellationToken);
            return result ?? "No alpine weather forecast data available.";
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }

    [McpServerTool(Name = "GetAvalancheWarnings"),
     Description("Get current avalanche (Lawine) warnings from DWD. " +
                 "Returns avalanche warning information for Alpine regions in Germany. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetAvalancheWarnings(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetAvalancheWarningsAsync(cancellationToken);
            return result ?? "No avalanche warning data available.";
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }
}
