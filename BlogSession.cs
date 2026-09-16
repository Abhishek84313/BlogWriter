using Newtonsoft.Json;

namespace BlogWriter;

/// <summary>Persisted state for one user's blog-writing conversation.</summary>
public sealed class BlogSession
{
    [JsonProperty("id")]
    public required string Id { get; init; }
    public string OwnerId { get; init; } = "";
    [JsonIgnore]
    public string? ETag { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public required ResearchState State { get; set; }
}

public sealed record BlogSessionSummary(
    string Id,
    string MainTask,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);