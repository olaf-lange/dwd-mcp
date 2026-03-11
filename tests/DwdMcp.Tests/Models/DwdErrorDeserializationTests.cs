using System.Text.Json;
using DwdMcp.Models;
using DwdMcp.Tests.Helpers;
using FluentAssertions;

namespace DwdMcp.Tests.Models;

public class DwdErrorDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void Deserialize_ValidJson_ReturnsDwdError()
    {
        var json = TestDataLoader.Load("DwdError.json");

        var result = JsonSerializer.Deserialize<DwdError>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Timestamp.Should().Be(1710100800000);
        result.Status.Should().Be(404);
        result.Error.Should().Be("Not Found");
        result.Message.Should().Contain("99999");
        result.Path.Should().Be("/v30/stationOverviewExtended");
    }

    [Fact]
    public void Deserialize_NullableProperties_DeserializeAsNull()
    {
        var json = """{"timestamp":null,"status":null,"error":null,"message":null,"path":null}""";

        var result = JsonSerializer.Deserialize<DwdError>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Timestamp.Should().BeNull();
        result.Status.Should().BeNull();
        result.Error.Should().BeNull();
        result.Message.Should().BeNull();
        result.Path.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ExtraProperties_AreIgnored()
    {
        var json = """{"status":500,"error":"Internal","extraField":"value"}""";

        var act = () => JsonSerializer.Deserialize<DwdError>(json, JsonOptions);

        act.Should().NotThrow();
    }
}
