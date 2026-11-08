#region Purpose
// Exception for a JSON-RPC response error or a session that cannot carry the request.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// A JSON-RPC response returned an error object, or the session refused the request.
/// </summary>
public sealed class JsonRpcException : Exception
{
  /// <summary>
  /// Initializes a new instance of the <see cref="JsonRpcException"/> class.
  /// </summary>
  public JsonRpcException()
    : base("JSON-RPC request failed")
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonRpcException"/> class.
  /// </summary>
  /// <param name="message">The exception message</param>
  public JsonRpcException(string message)
    : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonRpcException"/> class.
  /// </summary>
  /// <param name="message">The exception message</param>
  /// <param name="innerException">The exception that caused this failure</param>
  public JsonRpcException(string message, Exception innerException)
    : base(message, innerException)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonRpcException"/> class from a response error.
  /// </summary>
  /// <param name="error">The JSON-RPC error object</param>
  public JsonRpcException(JsonRpcError error)
    : base(error is null ? throw new ArgumentNullException(nameof(error)) : error.Message)
  {
    Error = error;
  }

  /// <summary>
  /// Gets the response error. Null when the failure is a session refusal or a frame that could not be read.
  /// </summary>
  public JsonRpcError? Error { get; }
}
