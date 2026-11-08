#region Purpose
// Do-activities for the JSON-RPC session machine.
#endregion

#region Design
// Only a do-activity may await. These methods capture nothing; the seam and the channel arrive on the context.
// StartChildAsync's bool result is applied on the pump, and that result behavior posts Started.
// RunReadyAsync drains the outbound channel and reads frames in one activity, because a state has one Do.
// It posts Response or ChildExited with Send. It does not write the context, and it does not call SendAsync.
// A null read is the child closing stdout. Any other read failure is thrown so Ready's OnFault runs.
// After ChildExited the activity waits on the token the exit cancels, so its own completion does not
// overtake that signal. Activity ends are drained before host signals.
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

  public static async Task StopChildAsync(JsonRpcSessionContext context, CancellationToken cancellationToken)
  {
    await context.JsonRpcProcessSeam.StopAsync(cancellationToken).ConfigureAwait(false);
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

        if (!JsonRpcFrames.TryReadId(frame, out string id))
        {
          throw new JsonException("a JSON-RPC frame has no id");
        }

        // Send posts and returns. SendAsync is the host's step. CA1849 treats the pair as blocking.
        #pragma warning disable CA1849
        context.Machine.Send(new Response(id, frame));
        #pragma warning restore CA1849
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
