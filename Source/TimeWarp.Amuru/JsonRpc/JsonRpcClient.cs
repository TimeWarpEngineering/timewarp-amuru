namespace TimeWarp.Amuru.JsonRpc;

/// <summary>
/// Implementation of JSON-RPC client for communicating with interactive processes.
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class JsonRpcClient : IJsonRpcClient
#pragma warning restore CA1812
{
    /// <summary>
    /// Initializes a new instance of the JsonRpcClient class.
    /// </summary>
    public JsonRpcClient()
    {
    }

    /// <inheritdoc />
    public Task<TResponse?> SendRequestAsync<TResponse>(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("JSON-RPC request sending not yet implemented");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }
}