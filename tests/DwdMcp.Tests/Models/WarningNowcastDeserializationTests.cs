using System.Text.Json;
using DwdMcp.Models;
using DwdMcp.Tests.Helpers;
using FluentAssertions;

namespace DwdMcp.Tests.Models;

public class WarningNowcastDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void Deserialize_ValidJson_ReturnsWarningNowcastResponse()
    {
        var json = TestDataLoader.Load("WarningNowcast.json");

        var result = JsonSerializer.Deserialize<WarningNowcastResponse>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Time.Should().Be(1710100800000);
        result.Warnings.Should().HaveCount(1);
        result.BinnenSee.Should().BeNull();
    }

    [Fact]
    public void Deserialize_Warning_ContainsRegionsAndPolygons()
    {
        var json = TestDataLoader.Load("WarningNowcast.json");
        var result = JsonSerializer.Deserialize<WarningNowcastResponse>(json, JsonOptions)!;

        var warning = result.Warnings![0];

        warning.Type.Should().Be(2);
        warning.Level.Should().Be(3);
        warning.Start.Should().Be(1710100800000);
        warning.End.Should().Be(1710111600000);
        warning.Event.Should().Be("STURMBÖEN");
        warning.HeadLine.Should().Contain("STURMBÖEN");
        warning.Regions.Should().HaveCount(1);
        warning.Regions![0].Polygon.Should().HaveCount(6);
        warning.Regions[0].Triangles.Should().HaveCount(3);
    }

    [Fact]
    public void Deserialize_NullableProperties_DeserializeAsNull()
    {
        var json = """{"time":null,"warnings":null,"binnenSee":null}""";

        var result = JsonSerializer.Deserialize<WarningNowcastResponse>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Time.Should().BeNull();
        result.Warnings.Should().BeNull();
        result.BinnenSee.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ExtraProperties_AreIgnored()
    {
        var json = """{"time":100,"warnings":[],"extraField":"ignored"}""";

        var act = () => JsonSerializer.Deserialize<WarningNowcastResponse>(json, JsonOptions);

        act.Should().NotThrow();
    }
}
