#region Purpose
// The real JSON-RPC process seam: one child, newline-delimited frames on stdin and stdout.
#endregion

#region Design
// Amuru creates the three anonymous pipes and passes the child ends through ProcessStartInfo
// handle properties. The parent keeps the other ends. CliWrap is not on this path.
// Stdout and stderr reads are asynchronous. Stderr is drained on its own task and never joined
// to the JSON-RPC frames. Blank lines are not frames.
// KillOnParentExit is set where that property exists. Stop sends SIGTERM on Unix, then
// SafeProcessHandle.WaitForExitOrKillOnCancellationAsync when the grace period or the caller token fires.
// ProcessExitStatus records an exit code or a terminating signal.
// ProcessStartInfo is banned for callers. This file is the same kind of boundary as TTY passthrough.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

using System.Diagnostics.CodeAnalysis;

[SuppressMessage(
  "Design",
  "CA1001",
  Justification = "StopAsync releases the process, pipes, and readers. JsonRpcClient.DisposeAsync is that path."
)]
internal sealed partial class JsonRpcChildProcess : IJsonRpcProcessSeam
{
  private readonly string Executable;
  private readonly string[] Arguments;
  private readonly string? WorkingDirectory;
  private readonly Dictionary<string, string?>? EnvironmentVariables;
  private readonly TimeSpan Grace;
  private readonly Action<string>? StandardErrorObserver;
  private readonly CancellationTokenSource StandardErrorCancellation = new();
  private Process? Child;
  private Stream? Input;
  private StreamReader? OutputReader;
  private StreamReader? ErrorReader;
  private Task? StandardErrorTask;
  private ProcessExitStatus? ExitStatusValue;
  private int Stopped;

  public JsonRpcChildProcess
  (
    string executable,
    string[] arguments,
    string? workingDirectory,
    Dictionary<string, string?>? environmentVariables,
    TimeSpan grace,
    Action<string>? standardErrorObserver
  )
  {
    ArgumentException.ThrowIfNullOrEmpty(executable);
    ArgumentNullException.ThrowIfNull(arguments);
    Executable = executable;
    Arguments = arguments;
    WorkingDirectory = workingDirectory;
    EnvironmentVariables = environmentVariables;
    Grace = grace;
    StandardErrorObserver = standardErrorObserver;
  }

  public ProcessExitStatus? ExitStatus => Volatile.Read(ref ExitStatusValue);

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    using PipeEnds pipes = OpenPipes();
    try
    {
      Child = StartChild(pipes);
      Input = new FileStream(pipes.ParentInput, FileAccess.Write, 4096, isAsync: true);
      Stream output = new FileStream(pipes.ParentOutput, FileAccess.Read, 4096, isAsync: true);
      Stream error = new FileStream(pipes.ParentError, FileAccess.Read, 4096, isAsync: true);
      OutputReader = new StreamReader(output, Encoding.UTF8);
      ErrorReader = new StreamReader(error, Encoding.UTF8);
      StandardErrorTask = ObserveStandardErrorAsync(StandardErrorCancellation.Token);
    }
    catch
    {
      await DisposeQuietlyAsync().ConfigureAwait(false);
      throw;
    }
  }

  public async Task WriteFrameAsync(string frame, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(frame);
    if (Input is null)
    {
      throw new InvalidOperationException("the child has not started");
    }

    byte[] bytes = Encoding.UTF8.GetBytes(frame);
    await Input.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
    await Input.FlushAsync(cancellationToken).ConfigureAwait(false);
  }

  public async Task<string?> ReadFrameAsync(CancellationToken cancellationToken)
  {
    if (OutputReader is null)
    {
      throw new InvalidOperationException("the child has not started");
    }

    while (true)
    {
      string? line = await OutputReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
      if (line is null)
      {
        await FinishAfterStdoutClosedAsync().ConfigureAwait(false);
        return null;
      }

      if (line.Length > 0)
      {
        return line;
      }
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    if (Interlocked.Exchange(ref Stopped, 1) != 0)
    {
      return;
    }

    await DisposeWriterAsync().ConfigureAwait(false);
    if (Child is not null && !HasExited())
    {
      TrySignalTerminate();
      using CancellationTokenSource grace = new();
      grace.CancelAfter(Grace);
      using var linked = CancellationTokenSource.CreateLinkedTokenSource
      (
        cancellationToken,
        grace.Token
      );
      await WaitOrKillAsync(linked.Token).ConfigureAwait(false);
    }

    RememberExitStatus();
    await DisposeReadersAsync().ConfigureAwait(false);
    await StandardErrorCancellation.CancelAsync().ConfigureAwait(false);
    if (StandardErrorTask is not null)
    {
      try
      {
        await StandardErrorTask.ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
      }
    }

    Child?.Dispose();
    Child = null;
  }

  [SuppressMessage(
    "Design",
    "CA1031",
    Justification = "A standard-error observer must not stop the drain. A full stderr pipe would stall the child."
  )]
  private async Task ObserveStandardErrorAsync(CancellationToken cancellationToken)
  {
    if (ErrorReader is null)
    {
      return;
    }

    try
    {
      while (true)
      {
        string? line = await ErrorReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        if (line is null)
        {
          return;
        }

        if (StandardErrorObserver is null)
        {
          continue;
        }

        try
        {
          StandardErrorObserver(line);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
          // Keep reading. The JSON-RPC stream is stdout.
        }
      }
    }
    catch (OperationCanceledException)
    {
    }
    catch (ObjectDisposedException)
    {
    }
    catch (IOException)
    {
    }
  }

  private Task FinishAfterStdoutClosedAsync()
  {
    if (Child is null)
    {
      return Task.CompletedTask;
    }

    if (!HasExited() && !OperatingSystem.IsIOS() && !OperatingSystem.IsTvOS())
    {
      if (Child.TryWaitForExitStatus(TimeSpan.FromMilliseconds(500), out ProcessExitStatus? status) && status is not null)
      {
        Volatile.Write(ref ExitStatusValue, status);
        return Task.CompletedTask;
      }

      try
      {
        Child.Kill(entireProcessTree: true);
      }
      catch (InvalidOperationException)
      {
      }
    }

    RememberExitStatus();
    return Task.CompletedTask;
  }

  private async Task WaitOrKillAsync(CancellationToken cancellationToken)
  {
    if (Child is null)
    {
      return;
    }

    if (OperatingSystem.IsIOS() || OperatingSystem.IsTvOS())
    {
      try
      {
        await Child.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
        TryKill();
        await Child.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
      }

      return;
    }

    try
    {
      ProcessExitStatus status = await Child.SafeHandle
        .WaitForExitOrKillOnCancellationAsync(cancellationToken)
        .ConfigureAwait(false);
      Volatile.Write(ref ExitStatusValue, status);
    }
    catch (InvalidOperationException)
    {
      RememberExitStatus();
    }
  }

  private void RememberExitStatus()
  {
    if (ExitStatus is not null || Child is null || OperatingSystem.IsIOS() || OperatingSystem.IsTvOS())
    {
      return;
    }

    if (Child.TryWaitForExitStatus(TimeSpan.FromSeconds(2), out ProcessExitStatus? status) && status is not null)
    {
      Volatile.Write(ref ExitStatusValue, status);
    }
  }

  private bool HasExited()
  {
    try
    {
      return Child is not null && Child.HasExited;
    }
    catch (InvalidOperationException)
    {
      return true;
    }
  }

  private void TrySignalTerminate()
  {
    if (Child is null)
    {
      return;
    }

    if (!OperatingSystem.IsLinux()
      && !OperatingSystem.IsMacOS()
      && !OperatingSystem.IsFreeBSD()
      && !OperatingSystem.IsAndroid())
    {
      return;
    }

    try
    {
      _ = Child.Signal(PosixSignal.SIGTERM);
    }
    catch (InvalidOperationException)
    {
    }
  }

  private void TryKill()
  {
    try
    {
      Child?.Kill(entireProcessTree: true);
    }
    catch (InvalidOperationException)
    {
    }
  }

  private async Task DisposeWriterAsync()
  {
    if (Input is null)
    {
      return;
    }

    await Input.DisposeAsync().ConfigureAwait(false);
    Input = null;
  }

  private async Task DisposeReadersAsync()
  {
    if (OutputReader is not null)
    {
      OutputReader.Dispose();
      OutputReader = null;
    }

    if (ErrorReader is not null)
    {
      ErrorReader.Dispose();
      ErrorReader = null;
    }

    await Task.CompletedTask.ConfigureAwait(false);
  }

  private async Task DisposeQuietlyAsync()
  {
    try
    {
      await StopAsync(CancellationToken.None).ConfigureAwait(false);
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
    }
  }

#pragma warning disable RS0030 // Banned symbol: the JSON-RPC channel owns ProcessStartInfo so it can redirect handle pipes.
  private Process StartChild(PipeEnds pipes)
  {
    ProcessStartInfo startInfo = new()
    {
      FileName = Executable,
      UseShellExecute = false,
      StandardInputHandle = pipes.ChildInput,
      StandardOutputHandle = pipes.ChildOutput,
      StandardErrorHandle = pipes.ChildError
    };
    if (!string.IsNullOrWhiteSpace(WorkingDirectory))
    {
      startInfo.WorkingDirectory = WorkingDirectory;
    }

    if (OperatingSystem.IsLinux() || OperatingSystem.IsWindows() || OperatingSystem.IsAndroid())
    {
      startInfo.KillOnParentExit = true;
    }

    foreach (string argument in Arguments)
    {
      startInfo.ArgumentList.Add(argument);
    }

    if (EnvironmentVariables is not null)
    {
      foreach (KeyValuePair<string, string?> pair in EnvironmentVariables)
      {
        if (pair.Value is null)
        {
          startInfo.Environment.Remove(pair.Key);
        }
        else
        {
          startInfo.Environment[pair.Key] = pair.Value;
        }
      }
    }

    Process process = new() { StartInfo = startInfo };
    if (!process.Start())
    {
      process.Dispose();
      throw new InvalidOperationException("the child process did not start");
    }

    pipes.DisposeChildEnds();
    return process;
  }
#pragma warning restore RS0030
}
