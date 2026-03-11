using System.ComponentModel;
using System.Text.Json;
using DwdMcp.Services;
using ModelContextProtocol.Server;

namespace DwdMcp.Tools;

[McpServerToolType]
public sealed class WarningTools
{
    private readonly IDwdApiClient _apiClient;

    public WarningTools(IDwdApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [McpServerTool(Name = "GetNowcastWarnings"),
     Description("Get current nowcast weather warnings from DWD in German. " +
                 "Nowcast warnings are short-term warnings for imminent severe weather events. " +
                 "Returns warning type, level, time range, affected regions with polygons, and description. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetNowcastWarnings(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetNowcastWarningsAsync(cancellationToken);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }

    [McpServerTool(Name = "GetNowcastWarningsEnglish"),
     Description("Get current nowcast weather warnings from DWD in English. " +
                 "Same data as GetNowcastWarnings but with English descriptions. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetNowcastWarningsEnglish(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetNowcastWarningsEnglishAsync(cancellationToken);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }

    [McpServerTool(Name = "GetMunicipalityWarnings"),
     Description("Get municipality-level (Gemeinde) severe weather warnings from DWD in German. " +
                 "Returns warnings including type, level, time range, affected regions, and descriptions. " +
                 "Also includes inland lake (Binnensee) warnings. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetMunicipalityWarnings(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetMunicipalityWarningsAsync(cancellationToken);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }

    [McpServerTool(Name = "GetMunicipalityWarningsEnglish"),
     Description("Get municipality-level (Gemeinde) severe weather warnings from DWD in English. " +
                 "Same data as GetMunicipalityWarnings but with English descriptions. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetMunicipalityWarningsEnglish(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetMunicipalityWarningsEnglishAsync(cancellationToken);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }

    [McpServerTool(Name = "GetCoastalWarnings"),
     Description("Get coastal (Küsten) severe weather warnings from DWD in German. " +
                 "Returns warnings for coastal regions keyed by region ID, including type, level, and descriptions. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetCoastalWarnings(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetCoastalWarningsAsync(cancellationToken);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }

    [McpServerTool(Name = "GetCoastalWarningsEnglish"),
     Description("Get coastal (Küsten) severe weather warnings from DWD in English. " +
                 "Same data as GetCoastalWarnings but with English descriptions. " +
                 "This is a read-only operation with no side effects.")]
    public async Task<string> GetCoastalWarningsEnglish(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiClient.GetCoastalWarningsEnglishAsync(cancellationToken);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (DwdApiException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, statusCode = ex.StatusCode });
        }
    }
}
