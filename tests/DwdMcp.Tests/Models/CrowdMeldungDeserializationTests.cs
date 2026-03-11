using System.Text.Json;
using DwdMcp.Models;
using DwdMcp.Tests.Helpers;
using FluentAssertions;

namespace DwdMcp.Tests.Models;

public class CrowdMeldungDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void Deserialize_ValidJson_ReturnsCrowdMeldungResponse()
    {
        var json = TestDataLoader.Load("CrowdMeldung.json");

        var result = JsonSerializer.Deserialize<CrowdMeldungResponse>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Start.Should().Be(1710057600000);
        result.End.Should().Be(1710100800000);
        result.HighestSeverities.Should().HaveCount(2);
        result.Meldungen.Should().HaveCount(2);
    }

    [Fact]
    public void Deserialize_Meldungen_ContainsCoordinatesAndCategories()
    {
        var json = TestDataLoader.Load("CrowdMeldung.json");
        var result = JsonSerializer.Deserialize<CrowdMeldungResponse>(json, JsonOptions)!;

        var meldung = result.Meldungen![0];

        meldung.MeldungId.Should().Be(12345);
        meldung.Lat.Should().Be("50.1109");
        meldung.Lon.Should().Be("8.6821");
        meldung.Place.Should().Be("Frankfurt am Main");
        meldung.Category.Should().Be("WIND");
        meldung.Auspraegung.Should().Be("SCHWER");
    }

    [Fact]
    public void Deserialize_HighestSeverities_ContainsCategoryAndAuspraegung()
    {
        var json = TestDataLoader.Load("CrowdMeldung.json");
        var result = JsonSerializer.Deserialize<CrowdMeldungResponse>(json, JsonOptions)!;

        result.HighestSeverities![0].Category.Should().Be("WIND");
        result.HighestSeverities[0].Auspraegung.Should().Be("SCHWER");
    }

    [Fact]
    public void Deserialize_NullableProperties_DeserializeAsNull()
    {
        var json = """{"start":null,"end":null,"highestSeverities":null,"meldungen":null}""";

        var result = JsonSerializer.Deserialize<CrowdMeldungResponse>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Start.Should().BeNull();
        result.Meldungen.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ExtraProperties_AreIgnored()
    {
        var json = """{"start":1,"end":2,"extraField":true,"meldungen":[]}""";

        var act = () => JsonSerializer.Deserialize<CrowdMeldungResponse>(json, JsonOptions);

        act.Should().NotThrow();
    }
}
