#region Purpose
// Timeout classification for CommandResult: the grace-period session, the process exit capture, and the timed stream.
#endregion

#region Design
// This file is the timeout half of CommandResult. The line-cap ratchet on command-result.cs cannot grow.
// CliWrap's cancellation exception has no exit code and the process is already disposed, so ProcessExitCapture
// copies the code from the Exited event. TimedOut is the grace-period kill only.
// A zero grace period is that kill. A positive grace period keeps the child's code, including Unix exit 130.
// An exit during that grace moves the forceful deadline to Timeout plus two grace periods. The drain stays bounded.
// An exit before the graceful signal leaves the original deadline. A later deadline cancel after either exit
// reports the captured code with TimedOut false. A zero grace period does not move the deadline.
// The caller token and an abandoned enumerator still cancel the drain.
// The timed stream cannot use ListenAsync: that event arrives only after ExecuteAsync returns a result.
// It writes each line into a one-slot channel and waits. CliWrap's pipe token cancels a blocked write
// when the run is abandoned or killed, so a slow consumer pushes back on the child instead of buffering the run.
#endregion

namespace TimeWarp.Amuru;

using System.Threading.Channels;

public partial class CommandResult
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "Disposing the enumerator must not replace the classified failure, and an abandoned stream must not surface the kill."
  )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Roslynator",
    "RCS1075",
    Justification = "The second await observes the same failure the iterator already threw, or the kill from an abandoned stream."
  )]
  private async IAsyncEnumerable<CommandEvent> ListenDuringTimeoutAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    // One waiting line. Stdout and stderr both write, so the channel is not single-writer.
    var channel = Channel.CreateBounded<CommandEvent>
    (
      new BoundedChannelOptions(1)
      {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true
      }
    );
    Command command = InternalCommand!
      .WithStandardOutputPipe
      (
        PipeTarget.Merge
        (
          InternalCommand.StandardOutputPipe,
          PipeTarget.ToDelegate
          (
            async (line, token) =>
            {
              await channel.Writer.WriteAsync(new StandardOutputCommandEvent(line), token).ConfigureAwait(false);
            },
            Encoding.Default
          )
        )
      )
      .WithStandardErrorPipe
      (
        PipeTarget.Merge
        (
          InternalCommand.StandardErrorPipe,
          PipeTarget.ToDelegate
          (
            async (line, token) =>
            {
              await channel.Writer.WriteAsync(new StandardErrorCommandEvent(line), token).ConfigureAwait(false);
            },
            Encoding.Default
          )
        )
      );

    CancellationTokenSource abandon = new();
    try
    {
      Task<CliExecution> execution = ExecuteCliAsync(command, cancellationToken, abandon.Token);
      Task completing = CompleteChannelWhenDoneAsync(execution, channel.Writer);
      bool finished = false;
      try
      {
        await foreach
        (
          CommandEvent commandEvent in channel.Reader.ReadAllAsync(CancellationToken.None).ConfigureAwait(false)
        )
        {
          yield return commandEvent;
        }

        CliExecution result = await execution.ConfigureAwait(false);
        finished = true;
        if (result.TimedOut)
        {
          RememberTimeout(string.Empty, string.Empty, result.RunTime, throwOnStrictTimeout: true);
          yield break;
        }

        Remember(new CommandOutput(string.Empty, string.Empty, result.ExitCode) { RunTime = result.RunTime });
      }
      finally
      {
        if (!finished)
        {
          await abandon.CancelAsync().ConfigureAwait(false);
          try
          {
            await execution.ConfigureAwait(false);
          }
          catch (Exception)
          {
            // The iterator already threw this failure, or the consumer left before completion.
          }
        }

        await completing.ConfigureAwait(false);
      }
    }
    finally
    {
      abandon.Dispose();
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "The channel has to close so the enumerator can classify the same task. The exception stays on that task."
  )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Roslynator",
    "RCS1075",
    Justification = "Swallowing here only unblocks the reader. ExecuteCliAsync's caller awaits the same task."
  )]
  private static async Task CompleteChannelWhenDoneAsync(Task execution, ChannelWriter<CommandEvent> writer)
  {
    try
    {
      await execution.ConfigureAwait(false);
    }
    catch (Exception)
    {
      // ExecuteCliAsync's caller classifies the same task. Completing the channel lets that await run.
    }
    finally
    {
      writer.TryComplete();
    }
  }

  private static async Task WaitForExitAfterKillAsync(Process process)
  {
    TryKill(process);
    using CancellationTokenSource bound = new(TimeSpan.FromSeconds(2));
    try
    {
      await process.WaitForExitAsync(bound.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
      // The process outlived the kill. The caller still reports timeout or cancellation.
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "Cancellation-token callback on a timer thread must never throw."
  )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Roslynator",
    "RCS1075",
    Justification = "Swallowing is the contract: the callback must never throw."
  )]
  private static void TryKill(Process process)
  {
    try
    {
      if (!process.HasExited)
      {
        process.Kill(entireProcessTree: true);
      }
    }
    catch (Exception)
    {
      // Runs as a cancellation-token callback on a timer thread: must never throw.
      // Covers already-exited processes, Win32/permission failures, and AggregateException from tree kill.
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "Cancellation-token callback on a timer thread must never throw."
  )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Roslynator",
    "RCS1075",
    Justification = "Swallowing is the contract: the callback must never throw."
  )]
  private static void TryInterrupt(Process process)
  {
    try
    {
      if (process.HasExited)
      {
        return;
      }

      // Windows has no SIGINT for a non-console child. The grace timer still force-kills it.
      if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
      {
        _ = NativeSignals.Kill(process.Id, NativeSignals.Interrupt);
      }
    }
    catch (Exception)
    {
      // Runs as a cancellation-token callback on a timer thread: must never throw.
    }
  }

  // CliWrap throws after it disposes the process, so the exit code has to be copied while Exited still can read it.
  private sealed class ProcessExitCapture
  {
    private readonly Action? ChildExited;
    private readonly TaskCompletionSource<int> ExitCodeSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ProcessExitCapture(Action? childExited = null)
    {
      ChildExited = childExited;
    }

    public void Attach(Process process)
    {
      process.EnableRaisingEvents = true;
      process.Exited += (_, _) => Publish(process);
      if (process.HasExited)
      {
        Publish(process);
      }
    }

    public async Task<int> ReadExitCodeAsync()
    {
      using CancellationTokenSource wait = new(TimeSpan.FromSeconds(2));
      try
      {
        return await ExitCodeSource.Task.WaitAsync(wait.Token).ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
        throw new InvalidOperationException(
          "The child exited during the timeout grace period, and its exit code was not captured.");
      }
    }

    private void Publish(Process process)
    {
      ChildExited?.Invoke();
      try
      {
        ExitCodeSource.TrySetResult(process.ExitCode);
      }
      catch (InvalidOperationException)
      {
        // ExitCode throws InvalidOperationException, and ObjectDisposedException derives from it,
        // once CliWrap has disposed the process. The read then times out instead of inventing 124.
      }
    }
  }

  private sealed class TimeoutSession : IDisposable
  {
    private readonly CancellationToken CallerToken;
    private readonly CancellationToken AbandonToken;
    private readonly TimeSpan Grace;
    private readonly CancellationTokenSource? GracefulSource;
    private readonly CancellationTokenSource? ForcefulDelaySource;
    private readonly CancellationTokenSource? LinkedForcefulSource;
    private readonly TimeSpan Limit;
    private readonly long StartedTimestamp;
    private int ChildExitedBeforeDeadlineFlag;

    public bool HasTimeout { get; }
    public CancellationToken GracefulToken { get; }
    public CancellationToken ForcefulToken { get; }
    public bool CallerCancelled => CallerToken.IsCancellationRequested;
    public bool Abandoned => AbandonToken.IsCancellationRequested;

    public bool GracefulSignaled =>
      HasTimeout
      && !CallerToken.IsCancellationRequested
      && (GracefulSource?.IsCancellationRequested ?? false);

    // A zero grace period has no cooperative window: the signal and the kill are one deadline.
    public bool GraceElapsed =>
      HasTimeout
      && !CallerToken.IsCancellationRequested
      && ((ForcefulDelaySource?.IsCancellationRequested ?? false)
        || (Grace == TimeSpan.Zero && GracefulSignaled));

    public bool TimedOut => GraceElapsed;

    // The direct child exited before the forceful deadline fired.
    public bool ChildExitedBeforeDeadline => Volatile.Read(ref ChildExitedBeforeDeadlineFlag) == 1;

    // A positive grace period keeps a wall-clock bound after the direct child exits.
    // An exit before the graceful signal leaves the deadline at Timeout plus grace.
    // An exit after that signal moves it to Timeout plus two grace periods.
    // An already-canceled source stays canceled, so a grace-period kill stays a timeout.
    // A zero grace period is the kill itself and does not move the deadline.
    public void NoteChildExit()
    {
      if (Grace <= TimeSpan.Zero || ForcefulDelaySource is not CancellationTokenSource forcefulDelaySource)
      {
        return;
      }

      try
      {
        if (forcefulDelaySource.IsCancellationRequested)
        {
          return;
        }

        if (GracefulSignaled)
        {
          TimeSpan target = AddGrace(AddGrace(Limit, Grace), Grace);
          TimeSpan elapsed = Stopwatch.GetElapsedTime(StartedTimestamp);
          TimeSpan remaining = target - elapsed;
          if (remaining > TimeSpan.Zero)
          {
            forcefulDelaySource.CancelAfter(remaining);
          }
        }

        if (!forcefulDelaySource.IsCancellationRequested)
        {
          Volatile.Write(ref ChildExitedBeforeDeadlineFlag, 1);
        }
      }
      catch (ObjectDisposedException)
      {
        // The exit event can be delivered after the session is disposed.
      }
    }

    public TimeoutSession(
      TimeSpan? timeout,
      TimeSpan grace,
      CancellationToken callerToken,
      CancellationToken abandonToken = default)
    {
      CallerToken = callerToken;
      AbandonToken = abandonToken;
      Grace = grace;
      if (timeout is not TimeSpan limit)
      {
        Limit = TimeSpan.Zero;
        StartedTimestamp = 0;
        GracefulToken = CancellationToken.None;
        ForcefulToken = callerToken;
        return;
      }

      HasTimeout = true;
      Limit = limit;
      StartedTimestamp = Stopwatch.GetTimestamp();
      GracefulSource = new CancellationTokenSource(limit);
      ForcefulDelaySource = new CancellationTokenSource(AddGrace(limit, grace));
      LinkedForcefulSource = abandonToken.CanBeCanceled
        ? CancellationTokenSource.CreateLinkedTokenSource(callerToken, ForcefulDelaySource.Token, abandonToken)
        : CancellationTokenSource.CreateLinkedTokenSource(callerToken, ForcefulDelaySource.Token);
      GracefulToken = GracefulSource.Token;
      ForcefulToken = LinkedForcefulSource.Token;
    }

    public void Dispose()
    {
      LinkedForcefulSource?.Dispose();
      ForcefulDelaySource?.Dispose();
      GracefulSource?.Dispose();
    }

    private static TimeSpan AddGrace(TimeSpan timeout, TimeSpan grace)
    {
      double milliseconds = timeout.TotalMilliseconds + grace.TotalMilliseconds;
      if (milliseconds > int.MaxValue)
      {
        return TimeSpan.FromMilliseconds(int.MaxValue);
      }

      return TimeSpan.FromMilliseconds(milliseconds);
    }
  }

  private static class NativeSignals
  {
    internal const int Interrupt = 2;

    // The libc import stays beside the grace-period kill that sends SIGINT.
    [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [System.Runtime.Versioning.SupportedOSPlatform("linux")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    internal static extern int Kill(int pid, int signal);
  }
}
