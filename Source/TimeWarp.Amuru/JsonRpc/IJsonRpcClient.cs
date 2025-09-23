namespace TimeWarp.Amuru.JsonRpc;

/// <summary>
/// Represents a JSON-RPC client for communicating with interactive processes.
/// </summary>
public interface IJsonRpcClient : IAsyncDisposable
{
    /// <summary>
    /// Sends a JSON-RPC request and waits for a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="method">The method name to call.</param>
    /// <param name="parameters">Optional parameters for the method.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the server.</returns>
    Task<TResponse?> SendRequestAsync<TResponse>(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default);
}