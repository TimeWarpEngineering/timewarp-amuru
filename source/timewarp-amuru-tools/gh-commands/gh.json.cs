#region Purpose
// Source-generated JSON metadata for the typed gh results, so they deserialize under Native AOT.
// Task 152.
#endregion

#region Design
// gh writes camelCase field names. Unknown fields are skipped, so a newer gh that adds fields
// still parses. Empty stdout reads as no value rather than a JsonException.
#endregion

namespace TimeWarp.Amuru;

using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// The <c>gh auth status --json hosts</c> document.
/// </summary>
internal sealed class GhAuthStatusDocument
{
  public Dictionary<string, List<GhAuthAccount>>? Hosts { get; init; }
}

/// <summary>
/// One account entry under a host in <c>gh auth status --json hosts</c>. The token is not part of it.
/// </summary>
internal sealed class GhAuthAccount
{
  public string? State { get; init; }

  public bool Active { get; init; }

  public string? Host { get; init; }

  public string? Login { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<GhWorkflowRun>))]
[JsonSerializable(typeof(GhPullRequest))]
[JsonSerializable(typeof(GhAuthStatusDocument))]
internal sealed partial class GhJsonContext : JsonSerializerContext;

/// <summary>
/// Deserialization helper for gh output.
/// </summary>
internal static class GhJson
{
  internal static T? Deserialize<T>(string json, JsonTypeInfo<T> typeInfo)
  {
    return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize(json, typeInfo);
  }
}
