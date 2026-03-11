using System.Text.Json.Serialization;

namespace DwdMcp.Models;

public sealed record DwdError
{
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; init; }

    [JsonPropertyName("status")]
    public int? Status { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("path")]
    public string? Path { get; init; }
}
