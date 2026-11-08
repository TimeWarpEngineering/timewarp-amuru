#region Purpose
// Newline-delimited JSON-RPC frames written with System.Text.Json.
#endregion

#region Design
// Utf8JsonWriter and JsonDocument stay inside the AOT-compatible surface.
// The outbound frame includes the trailing newline. TryReadId accepts that form and a bare line.
// A missing id is false so the Ready activity can throw and take OnFault.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

internal static class JsonRpcFrames
{
  public static string WriteRequest(string id, string method, string? paramsJson)
  {
    ArgumentException.ThrowIfNullOrEmpty(id);
    ArgumentException.ThrowIfNullOrEmpty(method);
    using MemoryStream stream = new();
    using (Utf8JsonWriter writer = new(stream))
    {
      writer.WriteStartObject();
      writer.WriteString("jsonrpc", "2.0");
      writer.WriteString("id", id);
      writer.WriteString("method", method);
      if (paramsJson is not null)
      {
        writer.WritePropertyName("params");
        writer.WriteRawValue(paramsJson);
      }

      writer.WriteEndObject();
    }

    return Encoding.UTF8.GetString(stream.ToArray()) + "\n";
  }

  public static bool TryReadId(string frame, out string id)
  {
    id = "";
    if (string.IsNullOrWhiteSpace(frame))
    {
      return false;
    }

    string json = frame.TrimEnd('\r', '\n');
    try
    {
      using var document = JsonDocument.Parse(json);
      if (!document.RootElement.TryGetProperty("id", out JsonElement idElement))
      {
        return false;
      }

      string? read = idElement.ValueKind switch
      {
        JsonValueKind.String => idElement.GetString(),
        JsonValueKind.Number => idElement.GetRawText(),
        _ => null
      };
      if (string.IsNullOrEmpty(read))
      {
        return false;
      }

      id = read;
      return true;
    }
    catch (JsonException)
    {
      return false;
    }
  }
}
