using System.Text.Json.Serialization;

namespace DwdMcp.Models;

public sealed record CoastWarning
{
    [JsonPropertyName("type")]
    public int? Type { get; init; }

    [JsonPropertyName("level")]
    public int? Level { get; init; }

    [JsonPropertyName("bn")]
    public bool? Bn { get; init; }

    [JsonPropertyName("instruction")]
    public string? Instruction { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("descriptionText")]
    public string? DescriptionText { get; init; }

    [JsonPropertyName("event")]
    public string? Event { get; init; }

    [JsonPropertyName("headline")]
    public string? Headline { get; init; }
}

public sealed record WarningCoastResponse
{
    [JsonPropertyName("time")]
    public long? Time { get; init; }

    [JsonPropertyName("warnings")]
    public Dictionary<string, List<CoastWarning>>? Warnings { get; init; }
}
