namespace TimeWarp.Multiavatar;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(Theme))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSourceGenerationOptions(WriteIndented = false)]
internal sealed partial class MultiavatarJsonContext : JsonSerializerContext
{
}