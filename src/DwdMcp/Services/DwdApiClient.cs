using System.Text.Json;
using DwdMcp.Models;
using Microsoft.Extensions.Logging;

namespace DwdMcp.Services;

public sealed class DwdApiClient : IDwdApiClient
{
    public const string StationHttpClientName = "DwdStation";
    public const string WarningHttpClientName = "DwdWarning";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DwdApiClient> _logger;

    public DwdApiClient(IHttpClientFactory httpClientFactory, ILogger<DwdApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<StationOverviewResponse> GetStationOverviewAsync(string[] stationIds, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(StationHttpClientName);
        var ids = string.Join(",", stationIds);
        var requestUri = $"stationOverviewExtended?stationIds={Uri.EscapeDataString(ids)}";

        _logger.LogDebug("Requesting station overview for IDs: {StationIds}", ids);

        var response = await client.GetAsync(requestUri, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var dict = JsonSerializer.Deserialize<Dictionary<string, StationData>>(json, JsonOptions);

        return new StationOverviewResponse
        {
            Stations = dict ?? new Dictionary<string, StationData>()
        };
    }

    public Task<CrowdMeldungResponse?> GetCrowdWeatherReportsAsync(CancellationToken cancellationToken = default)
        => GetWarningAsync<CrowdMeldungResponse>("crowd_meldungen_overview_v2.json", cancellationToken);

    public Task<WarningNowcastResponse?> GetNowcastWarningsAsync(CancellationToken cancellationToken = default)
        => GetWarningAsync<WarningNowcastResponse>("warnings_nowcast.json", cancellationToken);

    public Task<WarningNowcastResponse?> GetNowcastWarningsEnglishAsync(CancellationToken cancellationToken = default)
        => GetWarningAsync<WarningNowcastResponse>("warnings_nowcast_en.json", cancellationToken);

    public Task<GemeindeWarningsResponse?> GetMunicipalityWarningsAsync(CancellationToken cancellationToken = default)
        => GetWarningAsync<GemeindeWarningsResponse>("gemeinde_warnings_v2.json", cancellationToken);

    public Task<GemeindeWarningsResponse?> GetMunicipalityWarningsEnglishAsync(CancellationToken cancellationToken = default)
        => GetWarningAsync<GemeindeWarningsResponse>("gemeinde_warnings_v2_en.json", cancellationToken);

    public Task<WarningCoastResponse?> GetCoastalWarningsAsync(CancellationToken cancellationToken = default)
        => GetWarningAsync<WarningCoastResponse>("warnings_coast.json", cancellationToken);

    public Task<WarningCoastResponse?> GetCoastalWarningsEnglishAsync(CancellationToken cancellationToken = default)
        => GetWarningAsync<WarningCoastResponse>("warnings_coast_en.json", cancellationToken);

    public Task<string?> GetSeaWarningTextAsync(CancellationToken cancellationToken = default)
        => GetWarningTextAsync("sea_warning_text.json", cancellationToken);

    public Task<string?> GetAlpineWeatherForecastAsync(CancellationToken cancellationToken = default)
        => GetWarningTextAsync("alpen_forecast_text_dwms.json", cancellationToken);

    public Task<string?> GetAvalancheWarningsAsync(CancellationToken cancellationToken = default)
        => GetWarningTextAsync("warnings_lawine.json", cancellationToken);

    private async Task<T?> GetWarningAsync<T>(string path, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(WarningHttpClientName);

        _logger.LogDebug("Requesting warning data from {Path}", path);

        var response = await client.GetAsync(path, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);

        return await JsonSerializer.DeserializeAsync<T>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            JsonOptions,
            cancellationToken);
    }

    private async Task<string?> GetWarningTextAsync(string path, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(WarningHttpClientName);

        _logger.LogDebug("Requesting text data from {Path}", path);

        var response = await client.GetAsync(path, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        string errorBody;
        try
        {
            errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            errorBody = string.Empty;
        }

        DwdError? dwdError = null;
        try
        {
            dwdError = JsonSerializer.Deserialize<DwdError>(errorBody, JsonOptions);
        }
        catch
        {
            // Not a DWD error JSON — fall through
        }

        var message = dwdError?.Message ?? dwdError?.Error ?? errorBody;
        _logger.LogError("DWD API error {StatusCode} from {Url}: {Message}",
            (int)response.StatusCode, response.RequestMessage?.RequestUri, message);

        throw new DwdApiException((int)response.StatusCode, message);
    }
}

public sealed class DwdApiException : Exception
{
    public int StatusCode { get; }

    public DwdApiException(int statusCode, string? message)
        : base(message ?? $"DWD API returned status {statusCode}")
    {
        StatusCode = statusCode;
    }
}
