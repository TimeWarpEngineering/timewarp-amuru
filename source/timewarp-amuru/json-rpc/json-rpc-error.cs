#region Purpose
// A JSON-RPC 2.0 error object carried by a failed response.
#endregion

#region Design
// DataJson is the raw JSON of the optional data member. It stays a string so the value
// outlives the document that parsed the frame and so NativeAOT does not reflect over data.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// A JSON-RPC 2.0 error object from a response.
/// </summary>
public sealed class JsonRpcError
{
  /// <summary>
  /// Initializes a new instance of the <see cref="JsonRpcError"/> class.
  /// </summary>
  /// <param name="code">The JSON-RPC error code</param>
  /// <param name="message">The error message</param>
  /// <param name="dataJson">The raw JSON of the optional data member, or null when the member is absent</param>
  public JsonRpcError(int code, string message, string? dataJson)
  {
    ArgumentException.ThrowIfNullOrEmpty(message);
    Code = code;
    Message = message;
    DataJson = dataJson;
  }

  /// <summary>
  /// Gets the JSON-RPC error code.
  /// </summary>
  public int Code { get; }

  /// <summary>
  /// Gets the error message.
  /// </summary>
  public string Message { get; }

  /// <summary>
  /// Gets the raw JSON of the optional data member, or null when the response omitted it.
  /// </summary>
  public string? DataJson { get; }
}
