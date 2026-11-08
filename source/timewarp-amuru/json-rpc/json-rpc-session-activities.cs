#region Purpose
// Do-activities for the JSON-RPC session machine.
#endregion

#region Design
// Only a do-activity may await. These methods capture nothing; the seam and the channel arrive on the context.
// StartChildAsync's bool result is applied on the pump, and that result behavior posts Started.
// StopChildAsync's bool result posts ChildExited the same way. The host does not send either signal.
// RunReadyAsync drains the outbound channel and reads frames in one activity, because a state has one Do.
// It posts Response for a response frame, and ChildExited for a null read, with Send. It does not write the context.
// A notification, a null-id error, and a server request are ignored. A malformed frame is thrown so OnFault runs.
// After ChildExited from the read loop the activity waits on the token the exit cancels, so its own completion
// does not overtake that signal. Activity ends are drained before host signals.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

internal static class JsonRpcSessionActivities
{
  private static readonly Task Never =
    new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously).Task;

  public static async Task<bool> StartChildAsync(JsonRpcSessionContext context, CancellationToken cancellationToken)
  {
    await context.JsonRpcProcessSeam.StartAsync(cancellationToken).ConfigureAwait(false);
    return true;
  }

  public static async Task<bool> StopChildAsync(JsonRpcSessionContext context, CancellationToken cancellationToken)
  {
    await context.JsonRpcProcessSeam.StopAsync(cancellationToken).ConfigureAwait(false);
    return true;
  }

  public static async Task RunReadyAsync(JsonRpcSessionContext context, CancellationToken cancellationToken)
  {
    Task<string?> pendingRead = context.JsonRpcProcessSeam.ReadFrameAsync(cancellationToken);
    Task<bool>? outboundReady = null;
    while (!cancellationToken.IsCancellationRequested)
    {
      await DrainOutboundAsync(context, cancellationToken).ConfigureAwait(false);
      if (pendingRead.IsCompleted)
      {
        string? frame = await pendingRead.ConfigureAwait(false);
        if (frame is null)
        {
          // Send posts and returns. SendAsync is the host's step. CA1849 treats the pair as blocking.
          #pragma warning disable CA1849
          context.Machine.Send(new ChildExited());
          #pragma warning restore CA1849
          await Never.WaitAsync(cancellationToken).ConfigureAwait(false);
          return;
        }

        JsonRpcInbound inbound = JsonRpcFrames.ReadInbound(frame);
        if (inbound.Kind == JsonRpcInboundKind.Malformed)
        {
          throw new JsonException("a JSON-RPC frame is not a response");
        }

        if (inbound.Kind == JsonRpcInboundKind.Response)
        {
          // Send posts and returns. SendAsync is the host's step. CA1849 treats the pair as blocking.
          #pragma warning disable CA1849
          context.Machine.Send(new Response(inbound.Id, frame));
          #pragma warning restore CA1849
        }

        pendingRead = context.JsonRpcProcessSeam.ReadFrameAsync(cancellationToken);
        continue;
      }

      outboundReady ??= context.Outbound.Reader.WaitToReadAsync(cancellationToken).AsTask();
      Task<bool> ready = outboundReady;
      Task completed = await Task.WhenAny(pendingRead, ready).ConfigureAwait(false);
      if (ReferenceEquals(completed, ready))
      {
        bool canRead = await ready.ConfigureAwait(false);
        outboundReady = null;
        if (!canRead)
        {
          throw new InvalidOperationException("the outbound frame channel completed");
        }
      }
    }
  }

  private static async Task DrainOutboundAsync(JsonRpcSessionContext context, CancellationToken cancellationToken)
  {
    while (context.Outbound.Reader.TryRead(out string? frame))
    {
      if (frame is null)
      {
        continue;
      }

      await context.JsonRpcProcessSeam.WriteFrameAsync(frame, cancellationToken).ConfigureAwait(false);
    }
  }
}
