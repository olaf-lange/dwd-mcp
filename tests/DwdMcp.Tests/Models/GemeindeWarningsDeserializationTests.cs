using System.Text.Json;
using DwdMcp.Models;
using DwdMcp.Tests.Helpers;
using FluentAssertions;

namespace DwdMcp.Tests.Models;

public class GemeindeWarningsDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void Deserialize_ValidJson_ReturnsGemeindeWarningsResponse()
    {
        var json = TestDataLoader.Load("GemeindeWarnings.json");

        var result = JsonSerializer.Deserialize<GemeindeWarningsResponse>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Time.Should().Be(1710100800000);
        result.Warnings.Should().HaveCount(1);
    }

    [Fact]
    public void Deserialize_BinnenSee_ReturnsNestedStructure()
    {
        var json = TestDataLoader.Load("GemeindeWarnings.json");
        var result = JsonSerializer.Deserialize<GemeindeWarningsResponse>(json, JsonOptions)!;

        result.BinnenSee.Should().NotBeNull();
        result.BinnenSee.Should().ContainKey("501000000");
        var warnings = result.BinnenSee!["501000000"];
        warnings.Should().HaveCount(1);
        warnings[0].Event.Should().Be("STURM");
        warnings[0].Bn.Should().BeTrue();
        warnings[0].Headline.Should().Contain("Bodensee");
    }

    [Fact]
    public void Deserialize_Warning_ContainsAllFields()
    {
        var json = TestDataLoader.Load("GemeindeWarnings.json");
        var result = JsonSerializer.Deserialize<GemeindeWarningsResponse>(json, JsonOptions)!;

        var warning = result.Warnings![0];

        warning.Type.Should().Be(1);
        warning.Level.Should().Be(2);
        warning.IsVorabinfo.Should().BeTrue();
        warning.Event.Should().Be("FROST");
        warning.Regions.Should().HaveCount(1);
    }

    [Fact]
    public void Deserialize_NullableProperties_DeserializeAsNull()
    {
        var json = """{"time":null,"warnings":null,"binnenSee":null}""";

        var result = JsonSerializer.Deserialize<GemeindeWarningsResponse>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Time.Should().BeNull();
        result.Warnings.Should().BeNull();
        result.BinnenSee.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ExtraProperties_AreIgnored()
    {
        var json = """{"time":100,"warnings":[],"unknownProp":"val"}""";

        var act = () => JsonSerializer.Deserialize<GemeindeWarningsResponse>(json, JsonOptions);

        act.Should().NotThrow();
    }
}
