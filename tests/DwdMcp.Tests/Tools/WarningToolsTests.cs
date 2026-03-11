using DwdMcp.Models;
using DwdMcp.Services;
using DwdMcp.Tools;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DwdMcp.Tests.Tools;

public class WarningToolsTests
{
    private readonly IDwdApiClient _apiClient = Substitute.For<IDwdApiClient>();
    private readonly WarningTools _tools;

    public WarningToolsTests()
    {
        _tools = new WarningTools(_apiClient);
    }

    private static WarningNowcastResponse SampleNowcastResponse() => new()
    {
        Time = 1710100800000,
        Warnings = [new NowcastWarning { Event = "STURMBÖEN", Level = 3, HeadLine = "Warnung" }]
    };

    private static GemeindeWarningsResponse SampleGemeindeResponse() => new()
    {
        Time = 1710100800000,
        Warnings = [new GemeindeWarning { Event = "FROST", Level = 2 }]
    };

    private static WarningCoastResponse SampleCoastResponse() => new()
    {
        Time = 1710100800000,
        Warnings = new Dictionary<string, List<CoastWarning>>
        {
            ["NW_NORDSEE"] = [new CoastWarning { Event = "STURMFLUT", Level = 2 }]
        }
    };

    // --- Nowcast ---

    [Fact]
    public async Task GetNowcastWarnings_ReturnsWarningsInGerman()
    {
        _apiClient.GetNowcastWarningsAsync(Arg.Any<CancellationToken>()).Returns(SampleNowcastResponse());

        var result = await _tools.GetNowcastWarnings(CancellationToken.None);

        result.Should().Contain("STURMB\\u00D6EN");
        await _apiClient.Received(1).GetNowcastWarningsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNowcastWarningsEnglish_CallsEnglishEndpoint()
    {
        _apiClient.GetNowcastWarningsEnglishAsync(Arg.Any<CancellationToken>()).Returns(SampleNowcastResponse());

        await _tools.GetNowcastWarningsEnglish(CancellationToken.None);

        await _apiClient.Received(1).GetNowcastWarningsEnglishAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNowcastWarnings_ApiError_ReturnsErrorContent()
    {
        _apiClient.GetNowcastWarningsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DwdApiException(503, "Service unavailable"));

        var result = await _tools.GetNowcastWarnings(CancellationToken.None);

        result.Should().Contain("error");
        result.Should().Contain("503");
    }

    // --- Municipality ---

    [Fact]
    public async Task GetMunicipalityWarnings_ReturnsWarnings()
    {
        _apiClient.GetMunicipalityWarningsAsync(Arg.Any<CancellationToken>()).Returns(SampleGemeindeResponse());

        var result = await _tools.GetMunicipalityWarnings(CancellationToken.None);

        result.Should().Contain("FROST");
    }

    [Fact]
    public async Task GetMunicipalityWarningsEnglish_CallsEnglishEndpoint()
    {
        _apiClient.GetMunicipalityWarningsEnglishAsync(Arg.Any<CancellationToken>()).Returns(SampleGemeindeResponse());

        await _tools.GetMunicipalityWarningsEnglish(CancellationToken.None);

        await _apiClient.Received(1).GetMunicipalityWarningsEnglishAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMunicipalityWarnings_ApiError_ReturnsErrorContent()
    {
        _apiClient.GetMunicipalityWarningsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DwdApiException(500, "Error"));

        var result = await _tools.GetMunicipalityWarnings(CancellationToken.None);

        result.Should().Contain("error");
    }

    // --- Coastal ---

    [Fact]
    public async Task GetCoastalWarnings_ReturnsWarnings()
    {
        _apiClient.GetCoastalWarningsAsync(Arg.Any<CancellationToken>()).Returns(SampleCoastResponse());

        var result = await _tools.GetCoastalWarnings(CancellationToken.None);

        result.Should().Contain("STURMFLUT");
    }

    [Fact]
    public async Task GetCoastalWarningsEnglish_CallsEnglishEndpoint()
    {
        _apiClient.GetCoastalWarningsEnglishAsync(Arg.Any<CancellationToken>()).Returns(SampleCoastResponse());

        await _tools.GetCoastalWarningsEnglish(CancellationToken.None);

        await _apiClient.Received(1).GetCoastalWarningsEnglishAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCoastalWarnings_ApiError_ReturnsErrorContent()
    {
        _apiClient.GetCoastalWarningsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DwdApiException(404, "Not found"));

        var result = await _tools.GetCoastalWarnings(CancellationToken.None);

        result.Should().Contain("error");
        result.Should().Contain("404");
    }
}
