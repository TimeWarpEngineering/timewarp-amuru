#region Purpose
// Newline-delimited JSON-RPC frames written with System.Text.Json.
#endregion

#region Design
// Utf8JsonWriter and JsonDocument stay inside the AOT-compatible surface.
// The outbound frame includes the trailing newline. TryReadId accepts that form and a bare line.
// ReadInbound posts a Response only for a result or error object with no method and a string or number id.
// A method is a notification or a server request. A null id is an error that matches no client request.
// Those are ignored. A frame that is not a JSON object with a method or a result or an error is malformed.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

internal enum JsonRpcInboundKind
{
  Response,
  Ignored,
  Malformed,
}

internal readonly record struct JsonRpcInbound(JsonRpcInboundKind Kind, string Id);

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
    (bool parsed, string value) = Visit
    (
      frame,
      static root =>
      {
        bool read = TryReadId(root, out string found);
        return (read, found);
      },
      (false, "")
    );
    id = value;
    return parsed;
  }

  public static JsonRpcInbound ReadInbound(string frame) =>
    Visit
    (
      frame,
      static root => Classify(root),
      new JsonRpcInbound(JsonRpcInboundKind.Malformed, "")
    );

  private static JsonRpcInbound Classify(JsonElement root)
  {
    if (root.ValueKind != JsonValueKind.Object)
    {
      return new JsonRpcInbound(JsonRpcInboundKind.Malformed, "");
    }

    bool hasMethod = root.TryGetProperty("method", out JsonElement method)
      && method.ValueKind == JsonValueKind.String
      && !string.IsNullOrEmpty(method.GetString());
    bool hasBody = root.TryGetProperty("result", out _) || root.TryGetProperty("error", out _);
    if (hasMethod)
    {
      return new JsonRpcInbound(JsonRpcInboundKind.Ignored, "");
    }

    if (!hasBody)
    {
      return new JsonRpcInbound(JsonRpcInboundKind.Malformed, "");
    }

    if (!TryReadId(root, out string id))
    {
      return new JsonRpcInbound(JsonRpcInboundKind.Ignored, "");
    }

    return new JsonRpcInbound(JsonRpcInboundKind.Response, id);
  }

  private static T Visit<T>(string frame, Func<JsonElement, T> read, T failure)
  {
    if (string.IsNullOrWhiteSpace(frame))
    {
      return failure;
    }

    try
    {
      using var document = JsonDocument.Parse(frame.TrimEnd('\r', '\n'));
      return read(document.RootElement);
    }
    catch (JsonException)
    {
      return failure;
    }
  }

  private static bool TryReadId(JsonElement root, out string id)
  {
    id = "";
    if (root.ValueKind != JsonValueKind.Object
      || !root.TryGetProperty("id", out JsonElement idElement))
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
}
