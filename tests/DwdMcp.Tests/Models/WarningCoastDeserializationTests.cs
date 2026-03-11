using System.Text.Json;
using DwdMcp.Models;
using DwdMcp.Tests.Helpers;
using FluentAssertions;

namespace DwdMcp.Tests.Models;

public class WarningCoastDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void Deserialize_ValidJson_ReturnsWarningCoastResponse()
    {
        var json = TestDataLoader.Load("WarningCoast.json");

        var result = JsonSerializer.Deserialize<WarningCoastResponse>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Time.Should().Be(1710100800000);
        result.Warnings.Should().HaveCount(2);
    }

    [Fact]
    public void Deserialize_Warnings_AreKeyedByRegionId()
    {
        var json = TestDataLoader.Load("WarningCoast.json");
        var result = JsonSerializer.Deserialize<WarningCoastResponse>(json, JsonOptions)!;

        result.Warnings.Should().ContainKey("NW_NORDSEE");
        result.Warnings.Should().ContainKey("NW_OSTSEE");

        var nordsee = result.Warnings!["NW_NORDSEE"][0];
        nordsee.Type.Should().Be(3);
        nordsee.Level.Should().Be(2);
        nordsee.Event.Should().Be("STURMFLUT");
        nordsee.Headline.Should().Contain("Nordsee");
    }

    [Fact]
    public void Deserialize_NullableInstruction_DeserializesAsNull()
    {
        var json = TestDataLoader.Load("WarningCoast.json");
        var result = JsonSerializer.Deserialize<WarningCoastResponse>(json, JsonOptions)!;

        var ostsee = result.Warnings!["NW_OSTSEE"][0];
        ostsee.Instruction.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ExtraProperties_AreIgnored()
    {
        var json = """{"time":100,"warnings":{},"extra":"field"}""";

        var act = () => JsonSerializer.Deserialize<WarningCoastResponse>(json, JsonOptions);

        act.Should().NotThrow();
    }
}
