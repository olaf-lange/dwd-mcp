using System.Text.Json.Serialization;

namespace DwdMcp.Models;

public sealed record GemeindeWarning
{
    [JsonPropertyName("type")]
    public int? Type { get; init; }

    [JsonPropertyName("level")]
    public int? Level { get; init; }

    [JsonPropertyName("start")]
    public long? Start { get; init; }

    [JsonPropertyName("end")]
    public long? End { get; init; }

    [JsonPropertyName("regions")]
    public List<WarningRegion>? Regions { get; init; }

    [JsonPropertyName("urls")]
    public List<object>? Urls { get; init; }

    [JsonPropertyName("bn")]
    public bool? Bn { get; init; }

    [JsonPropertyName("isVorabinfo")]
    public bool? IsVorabinfo { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("descriptionText")]
    public string? DescriptionText { get; init; }

    [JsonPropertyName("event")]
    public string? Event { get; init; }

    [JsonPropertyName("headLine")]
    public string? HeadLine { get; init; }
}

public sealed record GemeindeWarningsResponse
{
    [JsonPropertyName("time")]
    public long? Time { get; init; }

    [JsonPropertyName("warnings")]
    public List<GemeindeWarning>? Warnings { get; init; }

    [JsonPropertyName("binnenSee")]
    public Dictionary<string, List<BinnenSeeWarning>>? BinnenSee { get; init; }
}

public sealed record BinnenSeeWarning
{
    [JsonPropertyName("type")]
    public int? Type { get; init; }

    [JsonPropertyName("level")]
    public int? Level { get; init; }

    [JsonPropertyName("start")]
    public long? Start { get; init; }

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
