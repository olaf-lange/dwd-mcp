using DwdMcp.Services;
using DwdMcp.Tools;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DwdMcp.Tests.Tools;

public class ForecastToolsTests
{
    private readonly IDwdApiClient _apiClient = Substitute.For<IDwdApiClient>();
    private readonly ForecastTools _tools;

    public ForecastToolsTests()
    {
        _tools = new ForecastTools(_apiClient);
    }

    // --- Sea Warning ---

    [Fact]
    public async Task GetSeaWarningText_ReturnsTextContent()
    {
        _apiClient.GetSeaWarningTextAsync(Arg.Any<CancellationToken>()).Returns("Sea warning bulletin text");

        var result = await _tools.GetSeaWarningText(CancellationToken.None);

        result.Should().Be("Sea warning bulletin text");
    }

    [Fact]
    public async Task GetSeaWarningText_NullResponse_ReturnsFallbackMessage()
    {
        _apiClient.GetSeaWarningTextAsync(Arg.Any<CancellationToken>()).Returns((string?)null);

        var result = await _tools.GetSeaWarningText(CancellationToken.None);

        result.Should().Contain("No sea warning data available");
    }

    [Fact]
    public async Task GetSeaWarningText_ApiError_ReturnsErrorContent()
    {
        _apiClient.GetSeaWarningTextAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DwdApiException(500, "Error"));

        var result = await _tools.GetSeaWarningText(CancellationToken.None);

        result.Should().Contain("error");
        result.Should().Contain("500");
    }

    // --- Alpine Forecast ---

    [Fact]
    public async Task GetAlpineWeatherForecast_ReturnsTextContent()
    {
        _apiClient.GetAlpineWeatherForecastAsync(Arg.Any<CancellationToken>()).Returns("Alpine forecast text");

        var result = await _tools.GetAlpineWeatherForecast(CancellationToken.None);

        result.Should().Be("Alpine forecast text");
    }

    [Fact]
    public async Task GetAlpineWeatherForecast_NullResponse_ReturnsFallbackMessage()
    {
        _apiClient.GetAlpineWeatherForecastAsync(Arg.Any<CancellationToken>()).Returns((string?)null);

        var result = await _tools.GetAlpineWeatherForecast(CancellationToken.None);

        result.Should().Contain("No alpine weather forecast data available");
    }

    [Fact]
    public async Task GetAlpineWeatherForecast_ApiError_ReturnsErrorContent()
    {
        _apiClient.GetAlpineWeatherForecastAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DwdApiException(503, "Service unavailable"));

        var result = await _tools.GetAlpineWeatherForecast(CancellationToken.None);

        result.Should().Contain("error");
    }

    // --- Avalanche ---

    [Fact]
    public async Task GetAvalancheWarnings_ReturnsTextContent()
    {
        _apiClient.GetAvalancheWarningsAsync(Arg.Any<CancellationToken>()).Returns("Avalanche warnings text");

        var result = await _tools.GetAvalancheWarnings(CancellationToken.None);

        result.Should().Be("Avalanche warnings text");
    }

    [Fact]
    public async Task GetAvalancheWarnings_NullResponse_ReturnsFallbackMessage()
    {
        _apiClient.GetAvalancheWarningsAsync(Arg.Any<CancellationToken>()).Returns((string?)null);

        var result = await _tools.GetAvalancheWarnings(CancellationToken.None);

        result.Should().Contain("No avalanche warning data available");
    }

    [Fact]
    public async Task GetAvalancheWarnings_ApiError_ReturnsErrorContent()
    {
        _apiClient.GetAvalancheWarningsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DwdApiException(404, "Not found"));

        var result = await _tools.GetAvalancheWarnings(CancellationToken.None);

        result.Should().Contain("error");
        result.Should().Contain("404");
    }
}
