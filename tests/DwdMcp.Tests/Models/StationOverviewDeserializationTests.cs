using System.Text.Json;
using DwdMcp.Models;
using DwdMcp.Tests.Helpers;
using FluentAssertions;

namespace DwdMcp.Tests.Models;

public class StationOverviewDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void Deserialize_ValidJson_ReturnsStationData()
    {
        var json = TestDataLoader.Load("StationOverview.json");

        var result = JsonSerializer.Deserialize<Dictionary<string, StationData>>(json, JsonOptions);

        result.Should().NotBeNull();
        result.Should().ContainKey("10865");
        result.Should().ContainKey("G005");
    }

    [Fact]
    public void Deserialize_Forecast1_ContainsExpectedFields()
    {
        var json = TestDataLoader.Load("StationOverview.json");
        var result = JsonSerializer.Deserialize<Dictionary<string, StationData>>(json, JsonOptions)!;

        var forecast = result["10865"].Forecast1;

        forecast.Should().NotBeNull();
        forecast!.StationId.Should().Be("10865");
        forecast.Start.Should().Be(1710100800000);
        forecast.TimeStep.Should().Be(3600);
        forecast.Temperature.Should().HaveCount(4);
        forecast.Temperature![3].Should().BeNull();
        forecast.WindSpeed.Should().Be("15");
        forecast.WindDirection.Should().Be("270");
        forecast.WindGust.Should().Be("30");
        forecast.Icon.Should().Contain(3);
        forecast.PrecipitationTotal.Should().HaveCount(4);
    }

    [Fact]
    public void Deserialize_Days_ContainsDayForecast()
    {
        var json = TestDataLoader.Load("StationOverview.json");
        var result = JsonSerializer.Deserialize<Dictionary<string, StationData>>(json, JsonOptions)!;

        var days = result["10865"].Days;

        days.Should().HaveCount(1);
        var day = days![0];
        day.StationId.Should().Be("10865");
        day.DayDate.Should().Be("2024-03-11");
        day.TemperatureMin.Should().Be(3);
        day.TemperatureMax.Should().Be(12);
        day.Icon.Should().Be(3);
        day.WindSpeed.Should().Be(15);
        day.WindGust.Should().Be(30);
        day.WindDirection.Should().Be(270);
        day.Sunshine.Should().Be(4);
    }

    [Fact]
    public void Deserialize_StationWithNullForecasts_HandlesGracefully()
    {
        var json = TestDataLoader.Load("StationOverview.json");
        var result = JsonSerializer.Deserialize<Dictionary<string, StationData>>(json, JsonOptions)!;

        var station = result["G005"];

        station.Forecast1.Should().BeNull();
        station.Forecast2.Should().BeNull();
        station.ForecastStart.Should().BeNull();
        station.Days.Should().BeEmpty();
        station.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_ExtraJsonProperties_AreIgnored()
    {
        var json = """{"10865":{"forecast1":null,"unknownField":"test","anotherExtra":42}}""";

        var act = () => JsonSerializer.Deserialize<Dictionary<string, StationData>>(json, JsonOptions);

        act.Should().NotThrow();
    }
}
