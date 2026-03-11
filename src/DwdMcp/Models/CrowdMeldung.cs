using System.Text.Json.Serialization;

namespace DwdMcp.Models;

public sealed record CrowdSeverity
{
    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("auspraegung")]
    public string? Auspraegung { get; init; }
}

public sealed record CrowdMeldung
{
    [JsonPropertyName("meldungId")]
    public int? MeldungId { get; init; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; init; }

    [JsonPropertyName("lat")]
    public string? Lat { get; init; }

    [JsonPropertyName("lon")]
    public string? Lon { get; init; }

    [JsonPropertyName("place")]
    public string? Place { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("auspraegung")]
    public string? Auspraegung { get; init; }

    [JsonPropertyName("zusatzAttribute")]
    public List<object>? ZusatzAttribute { get; init; }
}

public sealed record CrowdMeldungResponse
{
    [JsonPropertyName("start")]
    public long? Start { get; init; }

    [JsonPropertyName("end")]
    public long? End { get; init; }

    [JsonPropertyName("highestSeverities")]
    public List<CrowdSeverity>? HighestSeverities { get; init; }

    [JsonPropertyName("meldungen")]
    public List<CrowdMeldung>? Meldungen { get; init; }
}
