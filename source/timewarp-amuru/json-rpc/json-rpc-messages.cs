#region Purpose
// JSON-RPC request and response models owned by Amuru, plus the frame parser.
#endregion

#region Design
// The supported surface is request, response, and an outbound notification (no id).
// Inbound notifications, server requests, and proxy generation are outside it.
// Frames stay newline-delimited JSON.
// JsonDocument parses a response without reflecting over the caller's result type.
// The result payload is raw JSON so the caller serializer can read it.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

using TimeWarp.Amuru;

internal sealed class JsonRpcRequest
{
  public JsonRpcRequest(string id, string method, string? paramsJson)
  {
    ArgumentException.ThrowIfNullOrEmpty(id);
    ArgumentException.ThrowIfNullOrEmpty(method);
    Id = id;
    Method = method;
    ParamsJson = paramsJson;
  }

  public string Id { get; }

  public string Method { get; }

  public string? ParamsJson { get; }
}

internal readonly record struct JsonRpcParsedResponse(string? ResultJson, JsonRpcError? Error);

internal static class JsonRpcMessages
{
  public static JsonRpcParsedResponse Parse(string frame)
  {
    using var document = JsonDocument.Parse(frame);
    JsonElement root = document.RootElement;
    if (root.ValueKind != JsonValueKind.Object)
    {
      throw new JsonException("a JSON-RPC response is not an object");
    }

    if (root.TryGetProperty("error", out JsonElement error)
      && error.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
    {
      int code = error.TryGetProperty("code", out JsonElement codeElement) && codeElement.TryGetInt32(out int parsed)
        ? parsed
        : 0;
      string message = error.TryGetProperty("message", out JsonElement messageElement)
        && messageElement.ValueKind == JsonValueKind.String
        ? messageElement.GetString() ?? "JSON-RPC error"
        : "JSON-RPC error";
      string? dataJson = error.TryGetProperty("data", out JsonElement dataElement)
        && dataElement.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null
        ? dataElement.GetRawText()
        : null;
      return new JsonRpcParsedResponse(null, new JsonRpcError(code, message, dataJson));
    }

    if (!root.TryGetProperty("result", out JsonElement result))
    {
      throw new JsonException("a JSON-RPC response has no result");
    }

    return new JsonRpcParsedResponse(result.GetRawText(), null);
  }
}
