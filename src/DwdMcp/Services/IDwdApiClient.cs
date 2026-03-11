using DwdMcp.Models;

namespace DwdMcp.Services;

public interface IDwdApiClient
{
    Task<StationOverviewResponse> GetStationOverviewAsync(string[] stationIds, CancellationToken cancellationToken = default);
    Task<CrowdMeldungResponse?> GetCrowdWeatherReportsAsync(CancellationToken cancellationToken = default);
    Task<WarningNowcastResponse?> GetNowcastWarningsAsync(CancellationToken cancellationToken = default);
    Task<WarningNowcastResponse?> GetNowcastWarningsEnglishAsync(CancellationToken cancellationToken = default);
    Task<GemeindeWarningsResponse?> GetMunicipalityWarningsAsync(CancellationToken cancellationToken = default);
    Task<GemeindeWarningsResponse?> GetMunicipalityWarningsEnglishAsync(CancellationToken cancellationToken = default);
    Task<WarningCoastResponse?> GetCoastalWarningsAsync(CancellationToken cancellationToken = default);
    Task<WarningCoastResponse?> GetCoastalWarningsEnglishAsync(CancellationToken cancellationToken = default);
    Task<string?> GetSeaWarningTextAsync(CancellationToken cancellationToken = default);
    Task<string?> GetAlpineWeatherForecastAsync(CancellationToken cancellationToken = default);
    Task<string?> GetAvalancheWarningsAsync(CancellationToken cancellationToken = default);
}
