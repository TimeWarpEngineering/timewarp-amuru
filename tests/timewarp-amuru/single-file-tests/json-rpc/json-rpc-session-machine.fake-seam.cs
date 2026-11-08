#region Purpose
// Fake process seam for the JSON-RPC session machine tests.
#endregion

#region Design
// The suite compiles this file into the run-tests assembly. It is not a test class.
// HoldStart blocks StartAsync so a test can observe Starting. HoldStop blocks StopAsync so a test
// can observe Stopping before the activity completion posts ChildExited.
// Inbound frames, end of stream, and a thrown read are the three ways the Ready activity leaves the seam.
#endregion

namespace JsonRpcSessionMachine_;

using System.Threading.Channels;
using TimeWarp.Amuru.JsonRpc;

internal sealed class FakeJsonRpcProcessSeam : IJsonRpcProcessSeam
{
  private readonly TaskCompletionSource StartGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly TaskCompletionSource StopGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly Channel<ReadItem> Inbound = Channel.CreateUnbounded<ReadItem>();
  private readonly List<string> Written = [];
  private int StartCountValue;
  private int StopCountValue;
  private int HoldStopValue;

  public bool HoldStart { get; init; }

  public int StartCount => Volatile.Read(ref StartCountValue);

  public int StopCount => Volatile.Read(ref StopCountValue);

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    if (HoldStart)
    {
      await StartGate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    Interlocked.Increment(ref StartCountValue);
  }

  public Task WriteFrameAsync(string frame, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    lock (Written)
    {
      Written.Add(frame);
    }

    return Task.CompletedTask;
  }

  public async Task<string?> ReadFrameAsync(CancellationToken cancellationToken)
  {
    ReadItem item = await Inbound.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
    if (item.Error is not null)
    {
      throw item.Error;
    }

    return item.Frame;
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (Volatile.Read(ref HoldStopValue) == 1)
    {
      await StopGate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    Interlocked.Increment(ref StopCountValue);
  }

  public void HoldStop() => Volatile.Write(ref HoldStopValue, 1);

  public void ReleaseStop() => StopGate.TrySetResult();

  public void Enqueue(string frame) => Inbound.Writer.TryWrite(new ReadItem(frame, null));

  public void EnqueueEof() => Inbound.Writer.TryWrite(new ReadItem(null, null));

  public void EnqueueFault(Exception exception) => Inbound.Writer.TryWrite(new ReadItem(null, exception));

  public string[] Frames()
  {
    lock (Written)
    {
      return [.. Written];
    }
  }

  private readonly record struct ReadItem(string? Frame, Exception? Error);
}
