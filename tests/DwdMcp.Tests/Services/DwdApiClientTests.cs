using System.Net;
using DwdMcp.Services;
using DwdMcp.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace DwdMcp.Tests.Services;

public class DwdApiClientTests
{
    private static DwdApiClient CreateClient(
        MockHttpMessageHandler stationHandler,
        MockHttpMessageHandler warningHandler)
    {
        var factory = Substitute.For<IHttpClientFactory>();

        var stationClient = new HttpClient(stationHandler) { BaseAddress = new Uri("https://app-prod-ws.warnwetter.de/v30/") };
        var warningClient = new HttpClient(warningHandler) { BaseAddress = new Uri("https://s3.eu-central-1.amazonaws.com/app-prod-static.warnwetter.de/v16/") };

        factory.CreateClient(DwdApiClient.StationHttpClientName).Returns(stationClient);
        factory.CreateClient(DwdApiClient.WarningHttpClientName).Returns(warningClient);

        return new DwdApiClient(factory, NullLogger<DwdApiClient>.Instance);
    }

    private static DwdApiClient CreateClientWithStationHandler(MockHttpMessageHandler handler)
    {
        var warningHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
        return CreateClient(handler, warningHandler);
    }

    private static DwdApiClient CreateClientWithWarningHandler(MockHttpMessageHandler handler)
    {
        var stationHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
        return CreateClient(stationHandler, handler);
    }

    // --- Station endpoint tests ---

    [Fact]
    public async Task GetStationOverview_SingleId_ConstructsCorrectQueryString()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("StationOverview.json");
        var client = CreateClientWithStationHandler(handler);

        await client.GetStationOverviewAsync(["10865"]);

        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].RequestUri!.ToString().Should().Contain("stationIds=10865");
    }

    [Fact]
    public async Task GetStationOverview_MultipleIds_ConstructsCorrectQueryString()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("StationOverview.json");
        var client = CreateClientWithStationHandler(handler);

        await client.GetStationOverviewAsync(["10865", "G005"]);

        handler.Requests[0].RequestUri!.ToString().Should().Contain("stationIds=10865%2CG005");
    }

    [Fact]
    public async Task GetStationOverview_UsesStationBaseUrl()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("StationOverview.json");
        var client = CreateClientWithStationHandler(handler);

        await client.GetStationOverviewAsync(["10865"]);

        handler.Requests[0].RequestUri!.ToString().Should().StartWith("https://app-prod-ws.warnwetter.de/v30/");
    }

    [Fact]
    public async Task GetStationOverview_DeserializesIntoStronglyTypedModel()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("StationOverview.json");
        var client = CreateClientWithStationHandler(handler);

        var result = await client.GetStationOverviewAsync(["10865"]);

        result.Should().NotBeNull();
        result.Stations.Should().ContainKey("10865");
        result.Stations["10865"].Forecast1.Should().NotBeNull();
        result.Stations["10865"].Forecast1!.StationId.Should().Be("10865");
    }

    [Fact]
    public async Task GetStationOverview_Http404_ThrowsDwdApiExceptionWithStatusCode()
    {
        var errorJson = TestDataLoader.Load("DwdError.json");
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, errorJson);
        var client = CreateClientWithStationHandler(handler);

        var act = () => client.GetStationOverviewAsync(["99999"]);

        var ex = await act.Should().ThrowAsync<DwdApiException>();
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetStationOverview_Http500_ThrowsDwdApiExceptionWithMessage()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError,
            """{"timestamp":0,"status":500,"error":"Internal Server Error","message":"Server error","path":"/v30/stationOverviewExtended"}""");
        var client = CreateClientWithStationHandler(handler);

        var act = () => client.GetStationOverviewAsync(["10865"]);

        var ex = await act.Should().ThrowAsync<DwdApiException>();
        ex.Which.StatusCode.Should().Be(500);
        ex.Which.Message.Should().Contain("Server error");
    }

    [Fact]
    public async Task GetStationOverview_CancellationRequested_ThrowsOperationCanceled()
    {
        var handler = new MockHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(5000, ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = CreateClientWithStationHandler(handler);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => client.GetStationOverviewAsync(["10865"], cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // --- Warning endpoint tests ---

    [Fact]
    public async Task GetNowcastWarnings_UsesWarningBaseUrl()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("WarningNowcast.json");
        var client = CreateClientWithWarningHandler(handler);

        await client.GetNowcastWarningsAsync();

        handler.Requests[0].RequestUri!.ToString()
            .Should().StartWith("https://s3.eu-central-1.amazonaws.com/app-prod-static.warnwetter.de/v16/");
    }

    [Fact]
    public async Task GetNowcastWarnings_DeserializesCorrectly()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("WarningNowcast.json");
        var client = CreateClientWithWarningHandler(handler);

        var result = await client.GetNowcastWarningsAsync();

        result.Should().NotBeNull();
        result!.Time.Should().Be(1710100800000);
        result.Warnings.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCrowdWeatherReports_DeserializesCorrectly()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("CrowdMeldung.json");
        var client = CreateClientWithWarningHandler(handler);

        var result = await client.GetCrowdWeatherReportsAsync();

        result.Should().NotBeNull();
        result!.Meldungen.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMunicipalityWarnings_DeserializesCorrectly()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("GemeindeWarnings.json");
        var client = CreateClientWithWarningHandler(handler);

        var result = await client.GetMunicipalityWarningsAsync();

        result.Should().NotBeNull();
        result!.Warnings.Should().HaveCount(1);
        result.BinnenSee.Should().ContainKey("501000000");
    }

    [Fact]
    public async Task GetCoastalWarnings_DeserializesCorrectly()
    {
        var handler = MockHttpMessageHandler.WithJsonFile("WarningCoast.json");
        var client = CreateClientWithWarningHandler(handler);

        var result = await client.GetCoastalWarningsAsync();

        result.Should().NotBeNull();
        result!.Warnings.Should().ContainKey("NW_NORDSEE");
    }

    [Fact]
    public async Task GetSeaWarningText_ReturnsTextContent()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "\"Sea warning text content\"");
        var client = CreateClientWithWarningHandler(handler);

        var result = await client.GetSeaWarningTextAsync();

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAlpineWeatherForecast_ReturnsTextContent()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "\"Alpine forecast text\"");
        var client = CreateClientWithWarningHandler(handler);

        var result = await client.GetAlpineWeatherForecastAsync();

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAvalancheWarnings_ReturnsTextContent()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "\"Avalanche warning text\"");
        var client = CreateClientWithWarningHandler(handler);

        var result = await client.GetAvalancheWarningsAsync();

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetWarning_Http400_ThrowsDwdApiException()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.BadRequest,
            """{"status":400,"error":"Bad Request","message":"Invalid request"}""");
        var client = CreateClientWithWarningHandler(handler);

        var act = () => client.GetNowcastWarningsAsync();

        var ex = await act.Should().ThrowAsync<DwdApiException>();
        ex.Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetWarning_NonJsonErrorBody_StillThrowsDwdApiException()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "plain text error");
        var client = CreateClientWithWarningHandler(handler);

        var act = () => client.GetNowcastWarningsAsync();

        await act.Should().ThrowAsync<DwdApiException>();
    }

    [Fact]
    public async Task GetWarning_CancellationRequested_PropagatesCancellation()
    {
        var handler = new MockHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(5000, ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = CreateClientWithWarningHandler(handler);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => client.GetNowcastWarningsAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetWarning_TimeoutScenario_ThrowsTaskCanceled()
    {
        var handler = new MockHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(60), ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var factory = Substitute.For<IHttpClientFactory>();
        var warningClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com/"),
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        factory.CreateClient(DwdApiClient.WarningHttpClientName).Returns(warningClient);
        factory.CreateClient(DwdApiClient.StationHttpClientName).Returns(new HttpClient());

        var client = new DwdApiClient(factory, NullLogger<DwdApiClient>.Instance);

        var act = () => client.GetNowcastWarningsAsync();

        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
