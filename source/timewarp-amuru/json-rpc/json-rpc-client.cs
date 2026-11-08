#region Purpose
// Public JSON-RPC 2.0 request/response client over the session machine and a child process.
#endregion

#region Design
// SendAsync allocates a string id, sends the machine's Send signal, and waits on that entry's
// Completion. The machine's MachineStep is the refusal. A completion source is not added to the signal.
// After the payload is read, the client sends Release. Request timeout and caller cancellation both
// send RequestTimeout so the id does not stay pending.
// Both serializer paths resolve JsonTypeInfo and call the JsonTypeInfo overloads.
// A source-generated context is the NativeAOT path. Options supply a resolver, which may be
// that same context. There is no trim suppression on this path.
// NotifyAsync writes a frame with no id and does not create an in-flight entry.
// The host waits on Context.Ready and Context.Settled. Those complete in the transition effect,
// which runs before the state assignment, so the waiter yields until the state is published.
// Dispose bounds its wait by the child's shutdown grace plus a margin.
// The client does not dispatch server methods or inbound notifications.
#endregion

namespace TimeWarp.Amuru;

using System.Text.Json.Serialization.Metadata;
using TimeWarp.Amuru.JsonRpc;
using TimeWarp.Ramani;

/// <summary>
/// A JSON-RPC 2.0 client for one child process. Requests and responses travel on standard
/// input and standard output. Standard error stays off that stream.
/// </summary>
public sealed class JsonRpcClient : IAsyncDisposable
{
  private static readonly TimeSpan SettleMargin = TimeSpan.FromSeconds(2);
  private readonly JsonRpcChildProcess Child;
  private readonly JsonRpcSessionMachine Machine;
  private readonly JsonRpcSessionContext Context;
  private readonly TimeSpan? RequestTimeout;
  private readonly JsonSerializerOptions? SerializerOptions;
  private readonly JsonSerializerContext? SerializerContext;
  private long NextId;
  private int Disposed;

  private JsonRpcClient
  (
    JsonRpcChildProcess child,
    JsonRpcSessionMachine machine,
    JsonRpcSessionContext context,
    TimeSpan? requestTimeout,
    JsonSerializerOptions? serializerOptions,
    JsonSerializerContext? serializerContext
  )
  {
    Child = child;
    Machine = machine;
    Context = context;
    RequestTimeout = requestTimeout;
    SerializerOptions = serializerOptions;
    SerializerContext = serializerContext;
  }

  /// <summary>
  /// Gets how the child exited, once that status is known.
  /// <see cref="ProcessExitStatus.Signal"/> is set when a signal terminated the child.
  /// <see cref="ProcessExitStatus.Canceled"/> is set when shutdown killed the child because the
  /// grace period elapsed or the caller cancelled.
  /// </summary>
  public ProcessExitStatus? ExitStatus => Child.ExitStatus;

  /// <summary>
  /// Sends a request that has no parameters.
  /// </summary>
  /// <typeparam name="TResult">The result type</typeparam>
  /// <param name="method">The JSON-RPC method name</param>
  /// <returns>The deserialized result. Null when the response result is JSON null and the type allows it.</returns>
  /// <exception cref="JsonRpcException">The response is an error, the session refused the request, the serializer has no metadata for the parameter or result type, or the result could not be read.</exception>
  /// <exception cref="TimeoutException">The request exceeded the timeout from <see cref="ShellBuilder.WithTimeout"/>.</exception>
  /// <exception cref="OperationCanceledException">The caller's cancellation token fired.</exception>
  public Task<TResult?> SendAsync<TResult>(string method)
  {
    return SendAsync<TResult>(method, CancellationToken.None);
  }

  /// <summary>
  /// Sends a request that has no parameters.
  /// </summary>
  /// <typeparam name="TResult">The result type</typeparam>
  /// <param name="method">The JSON-RPC method name</param>
  /// <param name="cancellationToken">Cancels the wait for the response</param>
  /// <returns>The deserialized result. Null when the response result is JSON null and the type allows it.</returns>
  /// <exception cref="JsonRpcException">The response is an error, the session refused the request, the serializer has no metadata for the parameter or result type, or the result could not be read.</exception>
  /// <exception cref="TimeoutException">The request exceeded the timeout from <see cref="ShellBuilder.WithTimeout"/>.</exception>
  /// <exception cref="OperationCanceledException">The caller's cancellation token fired.</exception>
  public Task<TResult?> SendAsync<TResult>(string method, CancellationToken cancellationToken)
  {
    return SendAsync<object?, TResult>(method, null, cancellationToken);
  }

  /// <summary>
  /// Sends one request and waits for the correlated response.
  /// A null parameter value omits the params member.
  /// </summary>
  /// <typeparam name="TParams">The parameter type</typeparam>
  /// <typeparam name="TResult">The result type</typeparam>
  /// <param name="method">The JSON-RPC method name</param>
  /// <param name="parameters">The parameters. Null omits the params member.</param>
  /// <param name="cancellationToken">Cancels the wait for the response</param>
  /// <returns>The deserialized result. Null when the response result is JSON null and the type allows it.</returns>
  /// <exception cref="JsonRpcException">The response is an error, the session refused the request, the serializer has no metadata for the parameter or result type, or the result could not be read.</exception>
  /// <exception cref="TimeoutException">The request exceeded the timeout from <see cref="ShellBuilder.WithTimeout"/>.</exception>
  /// <exception cref="OperationCanceledException">The caller's cancellation token fired.</exception>
  public async Task<TResult?> SendAsync<TParams, TResult>
  (
    string method,
    TParams? parameters,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentException.ThrowIfNullOrEmpty(method);
    ObjectDisposedException.ThrowIf(Volatile.Read(ref Disposed) != 0, this);
    cancellationToken.ThrowIfCancellationRequested();
    string id = Interlocked.Increment(ref NextId).ToString(CultureInfo.InvariantCulture);
    string? paramsJson = SerializeParams(parameters);
    JsonRpcRequest request = new(id, method, paramsJson);
    MachineStep sent = await Machine.SendAsync(new Send(request.Id, request.Method, request.ParamsJson))
      .ConfigureAwait(false);
    ThrowIfRefused(sent);
    if (!Context.InFlight.TryGetValue(id, out JsonRpcInFlight? entry))
    {
      throw new JsonRpcException("the request was not accepted");
    }

    JsonRpcInFlightStatus status = await WaitForEntryAsync(entry, id, method, cancellationToken).ConfigureAwait(false);
    try
    {
      return ReadOutcome<TResult>(status, entry.Payload, method);
    }
    finally
    {
      await ReleaseFinishedAsync(id).ConfigureAwait(false);
    }
  }

  /// <summary>
  /// Sends a notification. The frame has no id, and the client does not wait for a response.
  /// </summary>
  /// <param name="method">The JSON-RPC method name</param>
  /// <exception cref="JsonRpcException">The session refused the notification.</exception>
  /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
  public Task NotifyAsync(string method)
  {
    return NotifyAsync<object?>(method, null, CancellationToken.None);
  }

  /// <summary>
  /// Sends a notification. The frame has no id, and the client does not wait for a response.
  /// </summary>
  /// <param name="method">The JSON-RPC method name</param>
  /// <param name="cancellationToken">Cancels the call before the session accepts it</param>
  /// <exception cref="JsonRpcException">The session refused the notification.</exception>
  /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
  /// <exception cref="OperationCanceledException">The caller's cancellation token fired.</exception>
  public Task NotifyAsync(string method, CancellationToken cancellationToken)
  {
    return NotifyAsync<object?>(method, null, cancellationToken);
  }

  /// <summary>
  /// Sends a notification. The frame has no id, and the client does not wait for a response.
  /// A null parameter value omits the params member.
  /// </summary>
  /// <typeparam name="TParams">The parameter type</typeparam>
  /// <param name="method">The JSON-RPC method name</param>
  /// <param name="parameters">The parameters. Null omits the params member.</param>
  /// <param name="cancellationToken">Cancels the call before the session accepts it</param>
  /// <exception cref="JsonRpcException">The session refused the notification, or the serializer has no metadata for the parameter type.</exception>
  /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
  /// <exception cref="OperationCanceledException">The caller's cancellation token fired.</exception>
  public async Task NotifyAsync<TParams>
  (
    string method,
    TParams? parameters,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentException.ThrowIfNullOrEmpty(method);
    ObjectDisposedException.ThrowIf(Volatile.Read(ref Disposed) != 0, this);
    cancellationToken.ThrowIfCancellationRequested();
    string? paramsJson = SerializeParams(parameters);
    MachineStep sent = await Machine.SendAsync(new Notify(method, paramsJson)).ConfigureAwait(false);
    ThrowIfRefused(sent);
  }

  /// <summary>
  /// Stops the child and releases the session.
  /// The wait for the session to settle is bounded by the shutdown grace plus a short margin.
  /// </summary>
  public async ValueTask DisposeAsync()
  {
    if (Interlocked.Exchange(ref Disposed, 1) != 0)
    {
      return;
    }

    MachineStep step = await Machine.SendAsync(new Dispose()).ConfigureAwait(false);
    if (step.Outcome == MachineStepOutcome.Refused)
    {
      await Child.StopAsync(CancellationToken.None).ConfigureAwait(false);
      return;
    }

    try
    {
      await Context.Settled.WaitAsync(Child.Grace + SettleMargin).ConfigureAwait(false);
    }
    catch (TimeoutException)
    {
      await Child.StopAsync(CancellationToken.None).ConfigureAwait(false);
    }
  }

  internal static async Task<JsonRpcClient> StartAsync
  (
    JsonRpcChildProcess child,
    TimeSpan? requestTimeout,
    JsonSerializerOptions? serializerOptions,
    JsonSerializerContext? serializerContext,
    CancellationToken cancellationToken
  )
  {
    JsonRpcSessionMachine machine = new();
    JsonRpcSessionContext context = new(child, machine);
    machine.Start(context);
    JsonRpcClient client = new(child, machine, context, requestTimeout, serializerOptions, serializerContext);
    try
    {
      MachineStep step = await machine.SendAsync(new Start()).ConfigureAwait(false);
      ThrowIfRefused(step);
      await WaitForReadyOrSettledAsync(context, cancellationToken).ConfigureAwait(false);
      await WaitUntilStatePublishedAsync(machine).ConfigureAwait(false);
      if (machine.State != JsonRpcSessionState.Ready)
      {
        throw new JsonRpcException(context.FaultReason ?? "the child did not become ready");
      }

      return client;
    }
    catch
    {
      if (machine.State == JsonRpcSessionState.Starting)
      {
        try
        {
          await Task.WhenAny(context.Ready, context.Settled)
            .WaitAsync(child.Grace + SettleMargin, CancellationToken.None)
            .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
        }

        await WaitUntilStatePublishedAsync(machine).ConfigureAwait(false);
      }

      await client.DisposeAsync().ConfigureAwait(false);
      throw;
    }
  }

  private async Task<JsonRpcInFlightStatus> WaitForEntryAsync
  (
    JsonRpcInFlight entry,
    string id,
    string method,
    CancellationToken cancellationToken
  )
  {
    using CancellationTokenSource timeout = new();
    if (RequestTimeout is TimeSpan limit)
    {
      timeout.CancelAfter(limit);
    }

    using var linked = CancellationTokenSource.CreateLinkedTokenSource
    (
      cancellationToken,
      timeout.Token
    );
    try
    {
      return await entry.Completion.WaitAsync(linked.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException exception)
    {
      if (!entry.Completion.IsCompleted)
      {
        await Machine.SendAsync(new RequestTimeout(id)).ConfigureAwait(false);
      }

      if (entry.Completion.IsCompletedSuccessfully)
      {
        JsonRpcInFlightStatus status = await entry.Completion.ConfigureAwait(false);
        if (status is JsonRpcInFlightStatus.Completed or JsonRpcInFlightStatus.Failed)
        {
          return status;
        }
      }

      await ReleaseFinishedAsync(id).ConfigureAwait(false);
      if (cancellationToken.IsCancellationRequested)
      {
        throw;
      }

      throw new TimeoutException($"the JSON-RPC request '{method}' timed out.", exception);
    }
  }

  private TResult? ReadOutcome<TResult>(JsonRpcInFlightStatus status, string? payload, string method)
  {
    switch (status)
    {
      case JsonRpcInFlightStatus.Completed:
        return ReadCompleted<TResult>(payload);
      case JsonRpcInFlightStatus.TimedOut:
        throw new TimeoutException($"the JSON-RPC request '{method}' timed out.");
      case JsonRpcInFlightStatus.Failed:
        throw new JsonRpcException(payload ?? "the JSON-RPC request failed");
      default:
        throw new InvalidOperationException("the JSON-RPC request is still pending");
    }
  }

  private TResult? ReadCompleted<TResult>(string? payload)
  {
    if (payload is null)
    {
      throw new JsonRpcException("the JSON-RPC response is missing");
    }

    try
    {
      JsonRpcParsedResponse parsed = JsonRpcMessages.Parse(payload);
      if (parsed.Error is not null)
      {
        throw new JsonRpcException(parsed.Error);
      }

      return DeserializeResult<TResult>(parsed.ResultJson ?? "null");
    }
    catch (Exception exception) when (exception is JsonException or InvalidOperationException or NotSupportedException)
    {
      throw new JsonRpcException("the JSON-RPC result could not be read", exception);
    }
  }

  private async Task ReleaseFinishedAsync(string id)
  {
    if (!Context.ContainsFinished(id))
    {
      return;
    }

    await Machine.SendAsync(new Release(id)).ConfigureAwait(false);
  }

  private string? SerializeParams<T>(T? value)
  {
    if (value is null)
    {
      return null;
    }

    try
    {
      return JsonSerializer.Serialize(value, TypeInfo(typeof(T)));
    }
    catch (Exception exception) when (exception is JsonException or InvalidOperationException or NotSupportedException)
    {
      throw new JsonRpcException("the JSON-RPC parameters could not be written", exception);
    }
  }

  private TResult? DeserializeResult<TResult>(string json)
  {
    return (TResult?)JsonSerializer.Deserialize(json, TypeInfo(typeof(TResult)));
  }

  private JsonTypeInfo TypeInfo(Type type)
  {
    if (SerializerContext is not null)
    {
      JsonTypeInfo? resolved = SerializerContext.GetTypeInfo(type);
      if (resolved is null)
      {
        throw new NotSupportedException($"the serializer context has no metadata for {type.FullName}");
      }

      return resolved;
    }

    if (SerializerOptions is null)
    {
      throw new InvalidOperationException("no JSON serializer was configured");
    }

    return SerializerOptions.GetTypeInfo(type);
  }

  private static void ThrowIfRefused(MachineStep step)
  {
    if (step.Outcome == MachineStepOutcome.Refused)
    {
      throw new JsonRpcException(step.Reason ?? "the JSON-RPC session refused the signal");
    }
  }

  private static async Task WaitForReadyOrSettledAsync
  (
    JsonRpcSessionContext context,
    CancellationToken cancellationToken
  )
  {
    Task completed = await Task.WhenAny(context.Ready, context.Settled)
      .WaitAsync(cancellationToken)
      .ConfigureAwait(false);
    await completed.ConfigureAwait(false);
  }

  private static async Task WaitUntilStatePublishedAsync(JsonRpcSessionMachine machine)
  {
    DateTime deadline = DateTime.UtcNow.AddSeconds(2);
    while (machine.State is JsonRpcSessionState.Starting or JsonRpcSessionState.Stopping)
    {
      if (DateTime.UtcNow > deadline)
      {
        return;
      }

      await Task.Yield();
    }
  }
}
