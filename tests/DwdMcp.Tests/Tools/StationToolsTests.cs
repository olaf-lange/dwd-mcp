using System.Text.Json;
using DwdMcp.Models;
using DwdMcp.Services;
using DwdMcp.Tools;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DwdMcp.Tests.Tools;

public class StationToolsTests
{
    private readonly IDwdApiClient _apiClient = Substitute.For<IDwdApiClient>();
    private readonly StationTools _tools;

    public StationToolsTests()
    {
        _tools = new StationTools(_apiClient);
    }

    [Fact]
    public async Task GetStationOverview_PassesStationIdsToApiClient()
    {
        _apiClient.GetStationOverviewAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>())
            .Returns(new StationOverviewResponse { Stations = new Dictionary<string, StationData>() });

        await _tools.GetStationOverview("10865,G005", CancellationToken.None);

        await _apiClient.Received(1).GetStationOverviewAsync(
            Arg.Is<string[]>(ids => ids.Length == 2 && ids[0] == "10865" && ids[1] == "G005"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStationOverview_ReturnsStructuredData()
    {
        var station = new StationData
        {
            Forecast1 = new ForecastData { StationId = "10865", Temperature = [5.2] },
            Days = [new DayForecast { StationId = "10865", TemperatureMax = 12 }]
        };
        _apiClient.GetStationOverviewAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>())
            .Returns(new StationOverviewResponse { Stations = new Dictionary<string, StationData> { ["10865"] = station } });

        var result = await _tools.GetStationOverview("10865", CancellationToken.None);

        result.Should().Contain("10865");
        var parsed = JsonSerializer.Deserialize<Dictionary<string, StationData>>(result);
        parsed.Should().ContainKey("10865");
    }

    [Fact]
    public async Task GetStationOverview_EmptyStationIds_ReturnsError()
    {
        var result = await _tools.GetStationOverview("", CancellationToken.None);

        result.Should().Contain("error");
        result.Should().Contain("No station IDs provided");
    }

    [Fact]
    public async Task GetStationOverview_ApiError_ReturnsErrorContent()
    {
        _apiClient.GetStationOverviewAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DwdApiException(404, "Station not found"));

        var result = await _tools.GetStationOverview("99999", CancellationToken.None);

        result.Should().Contain("error");
        result.Should().Contain("Station not found");
        result.Should().Contain("404");
    }

    [Fact]
    public async Task GetCrowdWeatherReports_ReturnsData()
    {
        _apiClient.GetCrowdWeatherReportsAsync(Arg.Any<CancellationToken>())
            .Returns(new CrowdMeldungResponse
            {
                Meldungen = [new CrowdMeldung { MeldungId = 1, Place = "Berlin", Category = "WIND" }]
            });

        var result = await _tools.GetCrowdWeatherReports(CancellationToken.None);

        result.Should().Contain("Berlin");
        result.Should().Contain("WIND");
    }

    [Fact]
    public async Task GetCrowdWeatherReports_ApiError_ReturnsErrorContent()
    {
        _apiClient.GetCrowdWeatherReportsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DwdApiException(500, "Internal server error"));

        var result = await _tools.GetCrowdWeatherReports(CancellationToken.None);

        result.Should().Contain("error");
        result.Should().Contain("500");
    }
}
