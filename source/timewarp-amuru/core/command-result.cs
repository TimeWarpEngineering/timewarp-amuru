#region Purpose
// Provides the execution surface over a pre-built command.
// Exposes shell-like run, capture, stream, pipe, selection, and passthrough behaviors from one fluent object.
#endregion

#region Design
// - CommandExtensions builds commands; this class executes them.
// - InternalCommand may be null, representing invalid construction or graceful degradation.
// - NullCommandResult is a shared sentinel to avoid repeated null-instance allocation.
// - Execution modes intentionally separate common scripting cases:
//   * RunAsync: stream to terminal
//   * CaptureAsync: capture silently
//   * RunAndCaptureAsync: stream and capture
//   * PassthroughAsync: interactive stream piping without true TTY
//   * TtyPassthroughAsync: real terminal inheritance for TUI apps
//   * SelectAsync: capture stdout while leaving interactive UI on stderr
// - Configured standard input is a string on the command, including empty (immediate EOF).
//   Null means the caller owns stdin. Capture, run, select, and stream modes pass that string through.
//   PassthroughAsync keeps it and sends stdout and stderr to the console. Console stdin is opened
//   only when no string was configured and the command is not a pipeline (the upstream stage owns
//   stdin there). TtyPassthroughAsync throws InvalidOperationException when a
//   string is configured: redirecting stdin would clear isatty on that stream, and inheriting the
//   console would drop the text with no error. CommandMock records the string on matched calls.
//   The TTY refusal runs before mock matching, so both paths refuse instead of dropping the text.
//   TtyPassthroughAsync also throws for Pipe compositions: it starts one process, so the upstream
//   stage would be dropped.
// - CaptureAsync and RunAndCaptureAsync set CommandOutput.RunTime from CliWrap, matching PassthroughAsync and TtyPassthroughAsync. Mocks and Empty stay at zero.
// - SelectAsync lets CommandExecutionException (zero-exit-code validation) and cancellation propagate; only unexpected runtime failures degrade to empty. TtyPassthroughAsync remains the documented validation exemption.
// - Streaming methods use CliWrap event streams for low-buffer processing.
// - Pipe composes commands by building the next stage and using CliWrap's pipe operator.
//   The options overload applies working directory, environment, and validation to that stage.
//   Upstream configured stdin stays the pipeline stdin.
// - Mock support applies to ALL execution modes (run, capture, stream, select, passthrough, TTY).
//   Strict mode (the default) throws on unmocked commands so tests can never silently run real processes.
//   Pipe compositions are the exception: they bypass mock matching (use MockBehavior.Loose for pipelines).
//   Piped CommandResults have no mock identity; ResolveMockSetup must not fall back to last-stage CliWrap TargetFilePath.
//   Matched calls record configured standard input (empty included). Null means none was configured.
// - Null commands never throw, preserving shell-like composition, but they report FAILURE:
//   NeverRanExitCode (-1) via ExitCode/Success so a command that never ran is distinguishable from one that succeeded.
// - Timeout is per command, stored with the options snapshot. CliWrap 3.10.5 ExecuteAsync(forceful, graceful)
//   sends SIGINT on the graceful token and kills on the forceful token. The graceful token fires at Timeout;
//   the forceful token fires at Timeout plus TimeoutGracePeriod, or immediately when the caller cancels.
//   Default validation records TimedOut and exit 124. Zero-exit validation throws TimeoutException.
//   The caller's own cancellation still propagates as OperationCanceledException and does not set TimedOut.
// - Pipe stages share one window: the pipeline keeps the shorter timeout and that stage's grace period.
// - TtyPassthroughAsync sends SIGINT itself on Linux and macOS, then kills the tree after the grace period.
//   Windows non-console children receive no SIGINT, so the grace period ends in the kill.
//   TtyPassthroughAsync still does not throw on timeout; it reports the result.
// - The instance is immutable aside from LastOutput. Execution methods may be called concurrently.
//   Each call returns its own CommandOutput, exit code, or streamed lines.
// - LastOutput is the most recent execution on this instance. It is not meaningful when the same
//   instance executes concurrently: the last writer wins. Prefer the returned CommandOutput.
//   StreamStdoutAsync, StreamStderrAsync, StreamCombinedAsync, and StreamToFileAsync do not return
//   CommandOutput, so LastOutput is their completion record (exit code, timeout, runtime).
//   It is set when an execution produces a result, including a timeout result (set before a strict TimeoutException).
//   It is not updated when the caller cancels, when any other exception escapes (strict non-zero exit
//   CommandExecutionException, mock Throws), or when a stream is abandoned before completion.
// - CaptureAsync and RunAndCaptureAsync retain every line plus the combined strings.
//   StreamStdoutAsync, StreamStderrAsync, and StreamCombinedAsync keep one line at a time. RunAsync and PassthroughAsync do not
//   buffer output. There is no size limit; the limit is process memory.
//   CaptureAsync records arrival order. RunAndCaptureAsync rebuilds OutputLines from separate strings,
//   so those lines are all stdout, then all stderr.
// - Streaming enumerators complete on timeout under default validation. LastOutput is set before the enumerator ends.
//   Real stream text stays on the enumerator. LastOutput records exit code, timeout, and runtime.
//   Disposing CliWrap's ListenAsync awaits the command task again. That second await throws the
//   termination OperationCanceledException; the dispose is swallowed so the classified result stands.
#endregion

#region Execution Modes
// Choose methods based on interaction model:
// - Non-interactive terminal output: RunAsync
// - Non-interactive data processing: CaptureAsync
// - Non-interactive logging + capture: RunAndCaptureAsync
// - Interactive stream-based tools like fzf: PassthroughAsync or SelectAsync
// - True TTY applications like vim/nano/edit: TtyPassthroughAsync
// - Large outputs: StreamStdoutAsync / StreamStderrAsync / StreamCombinedAsync
#endregion

#region Implementation Boundaries
// Most behavior is implemented with CliWrap pipes and event streams.
// TtyPassthroughAsync is the intentional exception: it uses Process/ProcessStartInfo
// because true TTY inheritance cannot be expressed through redirected pipes.
// That raw-process boundary is a core implementation detail, not a preferred public pattern.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// The execution surface over a built command: run, capture, stream, pipe, select,
/// and passthrough behaviors from one fluent object. Create via <see cref="Shell.Builder"/>
/// (then <c>Build()</c>) or <see cref="Shell.Run"/>.
/// A configured timeout ends the command with <see cref="CommandOutput.TimedOut"/> and
/// <see cref="TimeoutExitCode"/>. Strict validation throws <see cref="TimeoutException"/> instead.
/// <see cref="TtyPassthroughAsync"/> reports that result and does not throw.
/// Cancelling the caller's token throws <see cref="OperationCanceledException"/> and leaves <see cref="LastOutput"/> unchanged.
/// </summary>
/// <remarks>
/// The instance is immutable aside from <see cref="LastOutput"/>.
/// Execution methods may be called concurrently. Each call returns its own result.
/// <see cref="LastOutput"/> is the most recent execution on this instance.
/// It is not meaningful when the same instance executes concurrently. Prefer the returned <see cref="CommandOutput"/>.
/// </remarks>
public class CommandResult
{
  /// <summary>
  /// Exit code reported when a command never ran (invalid construction, failed pipe composition).
  /// Distinguishes never-ran from a successful (0) or tool-reported non-zero exit.
  /// </summary>
  public const int NeverRanExitCode = -1;

  /// <summary>
  /// Exit code reported when a command exceeds its timeout. Matches GNU timeout.
  /// </summary>
  public const int TimeoutExitCode = 124;

  private const string TtyConfiguredStandardInputMessage =
    "TtyPassthroughAsync requires console stdin; configured standard input cannot be used with a TTY";

  private const string TtyPipelineMessage =
    "TtyPassthroughAsync cannot run a pipeline; the upstream stage would be dropped";

  // Singleton for failed commands to avoid creating multiple identical null instances
  internal static readonly CommandResult NullCommandResult = new(null);

  // Property to access Command from other CommandResult instances in Pipe() method
  private Command? InternalCommand { get; }

  // Original executable/arguments as provided at construction, used for mock matching.
  // CliWrap's Arguments property is an escaped string; re-splitting it breaks on arguments
  // containing spaces, so the raw array must be carried through (null for piped compositions).
  private string? MockExecutable { get; }
  private string[]? MockArguments { get; }
  // Null: caller owns stdin. A string, including empty, is already attached to the CliWrap command.
  private string? ConfiguredStandardInput { get; }
  // True for Pipe compositions: the upstream stage owns stdin of the last stage.
  private bool IsPipeline { get; }

  private TimeSpan? Timeout { get; }
  private TimeSpan TimeoutGracePeriod { get; }
  private CommandResultValidation Validation { get; }

  /// <summary>
  /// Outcome of the most recent execution on this instance that produced a result, including a timeout result
  /// (set before a strict <see cref="TimeoutException"/> is thrown). Null before that first result.
  /// Streaming methods set this when the enumerator finishes; a stream abandoned before completion does not set it.
  /// It is not updated when the caller cancels or when any other exception escapes
  /// (for example a strict non-zero exit <c>CommandExecutionException</c> or a mock that throws).
  /// Not meaningful when the same instance executes concurrently: the last writer wins.
  /// Prefer the <see cref="CommandOutput"/> returned by the call.
  /// Stream methods that do not return <see cref="CommandOutput"/> record exit code, timeout, and runtime here.
  /// </summary>
  public CommandOutput? LastOutput { get; private set; }

  private CommandResult(
    Command? command,
    string? configuredStandardInput,
    bool isPipeline,
    string? mockExecutable,
    string[]? mockArguments,
    TimeSpan? timeout,
    TimeSpan timeoutGracePeriod,
    CommandResultValidation validation)
  {
    InternalCommand = command;
    ConfiguredStandardInput = configuredStandardInput;
    IsPipeline = isPipeline;
    MockExecutable = mockExecutable;
    MockArguments = mockArguments;
    Timeout = timeout;
    TimeoutGracePeriod = timeoutGracePeriod;
    Validation = validation;
  }

  internal CommandResult(Command? command)
    : this(
      command,
      configuredStandardInput: null,
      isPipeline: false,
      mockExecutable: null,
      mockArguments: null,
      timeout: null,
      timeoutGracePeriod: CommandOptions.DefaultTimeoutGracePeriod,
      validation: CommandResultValidation.None)
  {
  }

  internal CommandResult(
    Command? command,
    string executable,
    string[] arguments,
    string? configuredStandardInput,
    CommandOptions options)
    : this(
      command,
      configuredStandardInput,
      isPipeline: false,
      executable,
      arguments,
      options.Timeout,
      options.TimeoutGracePeriod,
      options.Validation ?? CommandResultValidation.None)
  {
  }

  /// <summary>
  /// Resolves the mock setup for this command, if mocking is enabled in the current context.
  /// Records the call on a match. In strict mode (the default), an unmatched command throws
  /// instead of falling through to real execution.
  /// </summary>
  private Testing.MockSetupData? ResolveMockSetup()
  {
    if (Testing.CommandMock.State is not { } state || InternalCommand == null)
    {
      return null;
    }

    // Piped compositions drop MockExecutable. Do not fall back to CliWrap's last-stage
    // identity — Setup("grep", "World") would otherwise match `echo … | grep World`.
    if (MockExecutable is null)
    {
      if (state.Behavior == Testing.MockBehavior.Strict)
      {
        throw new InvalidOperationException(
          "CommandMock strict mode: Pipe compositions bypass mock matching. " +
          "Use CommandMock.Enable(MockBehavior.Loose) to run the real pipeline.");
      }

      return null;
    }

    string executable = MockExecutable;
    string[] arguments = MockArguments ?? [];

    if (state.TryGetSetup(executable, arguments, out Testing.MockSetupData? setupData) && setupData != null)
    {
      state.RecordCall(executable, arguments, ConfiguredStandardInput);
      return setupData;
    }

    if (state.Behavior == Testing.MockBehavior.Strict)
    {
      string argumentText = arguments.Length > 0 ? " " + string.Join(' ', arguments) : string.Empty;
      throw new InvalidOperationException(
        $"CommandMock strict mode: no setup matches '{executable}{argumentText}'. " +
        "Add a matching CommandMock.Setup(...) or use CommandMock.Enable(MockBehavior.Loose) to allow real execution.");
    }

    return null;
  }

  private static async Task ApplyMockPreludeAsync(Testing.MockSetupData setupData, CancellationToken cancellationToken)
  {
    if (setupData.Delay.HasValue)
    {
      await Task.Delay(setupData.Delay.Value, cancellationToken).ConfigureAwait(false);
    }

    if (setupData.Exception != null)
    {
      throw setupData.Exception;
    }
  }

  private static string[] SplitMockLines(string? text) =>
    CommandOutput.SplitLines(text ?? string.Empty);

  private static TimeSpan? MergeTimeout(TimeSpan? upstream, TimeSpan? stage)
  {
    if (upstream is null)
    {
      return stage;
    }

    if (stage is null)
    {
      return upstream;
    }

    return upstream.Value <= stage.Value ? upstream : stage;
  }

  private static TimeSpan GraceForSharedTimeout(
    TimeSpan? upstreamTimeout,
    TimeSpan upstreamGrace,
    TimeSpan? stageTimeout,
    TimeSpan stageGrace,
    TimeSpan? sharedTimeout)
  {
    if (sharedTimeout is null || upstreamTimeout is null)
    {
      return stageTimeout is null ? upstreamGrace : stageGrace;
    }

    if (stageTimeout is null || upstreamTimeout.Value < stageTimeout.Value)
    {
      return upstreamGrace;
    }

    if (stageTimeout.Value < upstreamTimeout.Value)
    {
      return stageGrace;
    }

    return upstreamGrace <= stageGrace ? upstreamGrace : stageGrace;
  }

  private static CommandResultValidation MergeValidation(
    CommandResultValidation upstream,
    CommandResultValidation? stage)
  {
    CommandResultValidation resolvedStage = stage ?? CommandResultValidation.None;
    if (upstream == CommandResultValidation.ZeroExitCode || resolvedStage == CommandResultValidation.ZeroExitCode)
    {
      return CommandResultValidation.ZeroExitCode;
    }

    return CommandResultValidation.None;
  }

  private string DescribeTimeout()
  {
    if (Timeout is TimeSpan timeout)
    {
      string unit = timeout.TotalSeconds == 1 ? "second" : "seconds";
      return $"Command timed out after {timeout.TotalSeconds.ToString("G", CultureInfo.InvariantCulture)} {unit} and was terminated.";
    }

    return "Command timed out and was terminated.";
  }

  private CommandOutput Remember(CommandOutput output)
  {
    LastOutput = output;
    return output;
  }

  private CommandOutput RememberTimeout(string stdout, string stderr, TimeSpan runTime, bool throwOnStrictTimeout)
  {
    return RememberTimeout(
      new CommandOutput(stdout, stderr, TimeoutExitCode) { RunTime = runTime, TimedOut = true },
      throwOnStrictTimeout);
  }

  private CommandOutput RememberTimeout(CommandOutput output, bool throwOnStrictTimeout)
  {
    LastOutput = output;
    if (throwOnStrictTimeout && Validation == CommandResultValidation.ZeroExitCode)
    {
      throw new TimeoutException(DescribeTimeout());
    }

    return output;
  }

  private readonly struct CliExecution
  {
    public bool TimedOut { get; init; }
    public int ExitCode { get; init; }
    public TimeSpan RunTime { get; init; }
  }

  private async Task<CliExecution> ExecuteCliAsync(Command command, CancellationToken cancellationToken)
  {
    var stopwatch = Stopwatch.StartNew();
    using var session = new TimeoutSession(Timeout, TimeoutGracePeriod, cancellationToken);
    try
    {
      CliWrap.CommandResult result = session.HasTimeout
        ? await command.ExecuteAsync(session.ForcefulToken, session.GracefulToken).ConfigureAwait(false)
        : await command.ExecuteAsync(cancellationToken).ConfigureAwait(false);
      stopwatch.Stop();
      return new CliExecution
      {
        ExitCode = result.ExitCode,
        RunTime = result.RunTime
      };
    }
    catch (OperationCanceledException exception) when (session.CallerCancelled)
    {
      throw PropagateCallerCancellation(exception, cancellationToken);
    }
    catch (OperationCanceledException) when (session.TimedOut)
    {
      stopwatch.Stop();
      return new CliExecution
      {
        TimedOut = true,
        ExitCode = TimeoutExitCode,
        RunTime = stopwatch.Elapsed
      };
    }
  }

  private CommandOutput FinishCli(CliExecution execution, string stdout, string stderr, bool throwOnStrictTimeout)
  {
    if (execution.TimedOut)
    {
      return RememberTimeout(stdout, stderr, execution.RunTime, throwOnStrictTimeout);
    }

    return Remember(new CommandOutput(stdout, stderr, execution.ExitCode) { RunTime = execution.RunTime });
  }

  private CommandOutput FinishCli(CliExecution execution, IReadOnlyList<OutputLine> lines, bool throwOnStrictTimeout)
  {
    if (execution.TimedOut)
    {
      return RememberTimeout(new CommandOutput(lines, TimeoutExitCode) { RunTime = execution.RunTime, TimedOut = true }, throwOnStrictTimeout);
    }

    return Remember(new CommandOutput(lines, execution.ExitCode) { RunTime = execution.RunTime });
  }

  private readonly struct MockExecution
  {
    public bool Handled { get; init; }
    public bool TimedOut { get; init; }
    public Testing.MockSetupData? Setup { get; init; }
  }

  private async Task<MockExecution> TryExecuteMockAsync(CancellationToken cancellationToken)
  {
    Testing.MockSetupData? setup = ResolveMockSetup();
    if (setup is null)
    {
      return default;
    }

    cancellationToken.ThrowIfCancellationRequested();

    bool timesOut = setup.TimesOut
      || (Timeout is TimeSpan timeout && setup.Delay is TimeSpan delay && delay >= timeout);
    if (timesOut)
    {
      await WaitForMockTimeoutAsync(setup, cancellationToken).ConfigureAwait(false);
      return new MockExecution
      {
        Handled = true,
        TimedOut = true,
        Setup = setup
      };
    }

    await ApplyMockPreludeAsync(setup, cancellationToken).ConfigureAwait(false);
    return new MockExecution
    {
      Handled = true,
      Setup = setup
    };
  }

  private async Task WaitForMockTimeoutAsync(Testing.MockSetupData setup, CancellationToken cancellationToken)
  {
    TimeSpan wait = setup.Delay ?? TimeSpan.Zero;
    if (Timeout is TimeSpan timeout && wait > timeout)
    {
      wait = timeout;
    }

    if (wait <= TimeSpan.Zero)
    {
      return;
    }

    await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
  }

  private CommandOutput CompleteMock(
    Testing.MockSetupData setup,
    bool timedOut,
    bool throwOnStrictTimeout,
    bool includeText)
  {
    string stdout = includeText ? setup.Stdout ?? string.Empty : string.Empty;
    string stderr = includeText ? setup.Stderr ?? string.Empty : string.Empty;
    if (timedOut)
    {
      return RememberTimeout(stdout, stderr, TimeSpan.Zero, throwOnStrictTimeout);
    }

    return Remember(new CommandOutput(stdout, stderr, setup.ExitCode));
  }

  private static async Task WriteMockToTerminalAsync(Testing.MockSetupData setup)
  {
    if (!string.IsNullOrEmpty(setup.Stdout))
    {
      await TimeWarpTerminal.Default.WriteLineAsync(setup.Stdout).ConfigureAwait(false);
    }

    if (!string.IsNullOrEmpty(setup.Stderr))
    {
      await TimeWarpTerminal.Default.WriteErrorLineAsync(setup.Stderr).ConfigureAwait(false);
    }
  }

  private static OperationCanceledException PropagateCallerCancellation(
    OperationCanceledException exception,
    CancellationToken cancellationToken)
  {
    if (exception.CancellationToken == cancellationToken)
    {
      return exception;
    }

    return new OperationCanceledException(exception.Message, exception, cancellationToken);
  }

  private async IAsyncEnumerable<CommandEvent> ListenWithTimeoutAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    using var session = new TimeoutSession(Timeout, TimeoutGracePeriod, cancellationToken);
    var stopwatch = Stopwatch.StartNew();
    IAsyncEnumerable<CommandEvent> events = session.HasTimeout
      ? InternalCommand!.ListenAsync(Encoding.Default, Encoding.Default, session.ForcefulToken, session.GracefulToken)
      : InternalCommand!.ListenAsync(cancellationToken);

    // None keeps the tokens passed to ListenAsync. A caller token passed here is linked into the forceful token.
    IAsyncEnumerator<CommandEvent> enumerator = events.GetAsyncEnumerator(CancellationToken.None);
    try
    {
      bool timedOut = false;
      int exitCode = 0;
      while (true)
      {
        bool moved;
        try
        {
          moved = await enumerator.MoveNextAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException exception) when (session.CallerCancelled)
        {
          throw PropagateCallerCancellation(exception, cancellationToken);
        }
        catch (OperationCanceledException) when (session.TimedOut)
        {
          timedOut = true;
          break;
        }

        if (!moved)
        {
          break;
        }

        if (enumerator.Current is ExitedCommandEvent exited)
        {
          exitCode = exited.ExitCode;
        }

        yield return enumerator.Current;
      }

      stopwatch.Stop();
      if (timedOut)
      {
        RememberTimeout(string.Empty, string.Empty, stopwatch.Elapsed, throwOnStrictTimeout: true);
        yield break;
      }

      Remember(new CommandOutput(string.Empty, string.Empty, exitCode) { RunTime = stopwatch.Elapsed });
    }
    finally
    {
      // ListenAsync awaits the command task again while disposing. CliWrap 3.10.5 stores
      // graceful and forceful termination on that task as OperationCanceledException, and the
      // dispose path does not swallow a token it already translated. The loop above classified it.
      try
      {
        await enumerator.DisposeAsync().ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
      }
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

  private sealed class TimeoutSession : IDisposable
  {
    private readonly CancellationToken CallerToken;
    private readonly CancellationTokenSource? GracefulSource;
    private readonly CancellationTokenSource? ForcefulDelaySource;
    private readonly CancellationTokenSource? LinkedForcefulSource;

    public bool HasTimeout { get; }
    public CancellationToken GracefulToken { get; }
    public CancellationToken ForcefulToken { get; }
    public bool CallerCancelled => CallerToken.IsCancellationRequested;

    public bool TimedOut =>
      HasTimeout
      && !CallerToken.IsCancellationRequested
      && ((GracefulSource?.IsCancellationRequested ?? false) || (ForcefulDelaySource?.IsCancellationRequested ?? false));

    public TimeoutSession(TimeSpan? timeout, TimeSpan grace, CancellationToken callerToken)
    {
      CallerToken = callerToken;
      if (timeout is not TimeSpan limit)
      {
        GracefulToken = CancellationToken.None;
        ForcefulToken = callerToken;
        return;
      }

      HasTimeout = true;
      GracefulSource = new CancellationTokenSource(limit);
      ForcefulDelaySource = new CancellationTokenSource(AddGrace(limit, grace));
      LinkedForcefulSource = CancellationTokenSource.CreateLinkedTokenSource(callerToken, ForcefulDelaySource.Token);
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

    // DllImport stays here so CommandResult does not have to be partial for one libc call.
    [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [System.Runtime.Versioning.SupportedOSPlatform("linux")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    internal static extern int Kill(int pid, int signal);
  }

  /// <summary>
  /// Passes the command through to the console by piping stdin/stdout/stderr streams.
  /// This allows interactive commands like fzf to work with user input and terminal UI.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This method uses CliWrap's stream piping which does NOT preserve TTY characteristics.
  /// For TUI applications like vim, nano, or edit that require a real TTY, use
  /// <see cref="TtyPassthroughAsync"/> instead.
  /// </para>
  /// <para>
  /// Use this method for:
  /// - Interactive filter tools like fzf
  /// - Simple interactive prompts
  /// - Commands that don't check isatty()
  /// </para>
  /// <para>
  /// Use TtyPassthroughAsync for:
  /// - Full-screen TUI editors (vim, nano, edit)
  /// - SSH sessions with terminal allocation
  /// - Applications that call isatty() to verify terminal
  /// </para>
  /// <para>
  /// When standard input was configured (including an empty string, which is immediate EOF),
  /// that text is the child's stdin. Stdout and stderr still go to the console.
  /// Console stdin is opened only when no standard input was configured. For a
  /// <see cref="Pipe(string, string[])"/> composition, the upstream stage stays the stdin.
  /// </para>
  /// <para>
  /// Caveat: when stdin comes from the console, a child that exits without draining it can leave
  /// a pending console read that swallows the parent's next line of input.
  /// Prefer TtyPassthroughAsync (stream inheritance, no pending reads) when the child may
  /// ignore stdin.
  /// </para>
  /// </remarks>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout.</exception>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return Remember(CommandOutput.Empty(NeverRanExitCode));
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      await WriteMockToTerminalAsync(setup).ConfigureAwait(false);
      return CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: false);
    }

    // Open console streams for interactive piping
    Stream stdOut = TimeWarpTerminal.Default.OpenStandardOutput();
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable stdOutScope = stdOut.ConfigureAwait(false);
    Stream stdErr = TimeWarpTerminal.Default.OpenStandardError();
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable stdErrScope = stdErr.ConfigureAwait(false);

    // Keep a configured string pipe (including empty) or the upstream pipe stage.
    // Replace stdin only when the caller owns it.
    Command interactiveCommand = InternalCommand
      .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
      .WithStandardErrorPipe(PipeTarget.ToStream(stdErr));

    Stream? stdIn = null;
    if (ConfiguredStandardInput is null && !IsPipeline)
    {
      stdIn = TimeWarpTerminal.Default.OpenStandardInput();
      interactiveCommand = interactiveCommand.WithStandardInputPipe(PipeSource.FromStream(stdIn));
    }

    CliExecution execution;
    try
    {
      execution = await ExecuteCliAsync(interactiveCommand, cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      if (stdIn is not null)
      {
        await stdIn.DisposeAsync().ConfigureAwait(false);
      }
    }

    return FinishCli(execution, string.Empty, string.Empty, throwOnStrictTimeout: true);
  }

  /// <summary>
  /// Executes the command with true TTY passthrough for TUI applications.
  /// Unlike PassthroughAsync which pipes Console streams, this method
  /// uses Process.Start directly without any stream redirection, allowing
  /// the child process to inherit the terminal's TTY characteristics.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Use this method for TUI applications like vim, nano, edit, etc. that
  /// require a real terminal (TTY) to function properly. These applications
  /// check isatty() and fail when stdin/stdout/stderr are pipes.
  /// </para>
  /// <para>
  /// Note: Output cannot be captured with this method since streams are
  /// not redirected. Use PassthroughAsync for non-TUI interactive commands
  /// where you want stream access.
  /// </para>
  /// <para>
  /// Configured standard input is refused. Redirecting only stdin would make isatty fail on
  /// that stream, and leaving stdin inherited would discard the configured text.
  /// </para>
  /// <para>
  /// Validation options do not apply here: this method never throws on a
  /// non-zero exit code (even with WithZeroExitCodeValidation); inspect
  /// the returned CommandOutput's ExitCode/Success instead.
  /// A timeout is reported the same way: <see cref="CommandOutput.TimedOut"/> and exit
  /// <see cref="TimeoutExitCode"/>, with no exception.
  /// On Linux and macOS the child receives SIGINT when the timeout elapses, then a tree kill after the grace period.
  /// Windows non-console children receive no SIGINT, so the grace period ends in that kill.
  /// </para>
  /// </remarks>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <exception cref="InvalidOperationException">
  /// Standard input was configured. The message is
  /// "TtyPassthroughAsync requires console stdin; configured standard input cannot be used with a TTY".
  /// Also thrown for a <see cref="Pipe(string, string[])"/> composition, which this method cannot run.
  /// </exception>
  /// <returns>The execution result (output strings will be empty since streams are inherited)</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return Remember(CommandOutput.Empty(NeverRanExitCode));
    }

    if (ConfiguredStandardInput is not null)
    {
      throw new InvalidOperationException(TtyConfiguredStandardInputMessage);
    }

    if (IsPipeline)
    {
      throw new InvalidOperationException(TtyPipelineMessage);
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      return CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: false, includeText: false);
    }

    DateTimeOffset startTime = DateTimeOffset.Now;

#pragma warning disable RS0030 // Banned symbol: Amuru itself must use Process/ProcessStartInfo to implement TTY passthrough
    using var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = InternalCommand.TargetFilePath,
        Arguments = InternalCommand.Arguments,
        WorkingDirectory = string.IsNullOrEmpty(InternalCommand.WorkingDirPath)
          ? null
          : InternalCommand.WorkingDirPath,
        UseShellExecute = false,
        // CRITICAL: Do NOT redirect any streams - this preserves TTY inheritance
        RedirectStandardInput = false,
        RedirectStandardOutput = false,
        RedirectStandardError = false
      }
    };

    // Apply environment variables if configured
    if (InternalCommand.EnvironmentVariables.Count > 0)
    {
      foreach (KeyValuePair<string, string?> envVar in InternalCommand.EnvironmentVariables)
      {
        if (envVar.Value != null)
        {
          process.StartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
        }
        else
        {
          process.StartInfo.EnvironmentVariables.Remove(envVar.Key);
        }
      }
    }

    process.Start();
#pragma warning restore RS0030

    using var session = new TimeoutSession(Timeout, TimeoutGracePeriod, cancellationToken);
    using CancellationTokenRegistration gracefulRegistration = session.GracefulToken.CanBeCanceled
      ? session.GracefulToken.Register(() => TryInterrupt(process))
      : default;
    using CancellationTokenRegistration forcefulRegistration = session.ForcefulToken.CanBeCanceled
      ? session.ForcefulToken.Register(() => TryKill(process))
      : default;

    try
    {
      await process.WaitForExitAsync(session.ForcefulToken).ConfigureAwait(false);
    }
    catch (OperationCanceledException exception) when (session.CallerCancelled)
    {
      await WaitForExitAfterKillAsync(process).ConfigureAwait(false);
      throw PropagateCallerCancellation(exception, cancellationToken);
    }
    catch (OperationCanceledException) when (session.TimedOut)
    {
      // The forceful token fired after the grace period. Kill again in case the callback lost the race.
      await WaitForExitAfterKillAsync(process).ConfigureAwait(false);
    }

    TimeSpan runTime = DateTimeOffset.Now - startTime;
    if (session.TimedOut)
    {
      return RememberTimeout(string.Empty, string.Empty, runTime, throwOnStrictTimeout: false);
    }

    return Remember(new CommandOutput(string.Empty, string.Empty, process.ExitCode) { RunTime = runTime });
  }

  /// <summary>
  /// Executes an interactive selection command and returns the selected value.
  /// This is ideal for commands like fzf where the UI is rendered to stderr but the selection is written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected value from the interactive command</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout.</exception>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI boundary: interactive selection should degrade gracefully and return empty result on unexpected runtime failures."
  )]
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      Remember(CommandOutput.Empty(NeverRanExitCode));
      return string.Empty;
    }

    // Mock resolution happens outside the graceful-degradation try below so strict-mode
    // violations and configured mock exceptions propagate to the test instead of being swallowed.
    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      CommandOutput output = CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: true);
      return output.Stdout.TrimEnd('\n', '\r');
    }

    // Use StringBuilder to capture output
    StringBuilder outputBuilder = new();
    Stream stdErr = TimeWarpTerminal.Default.OpenStandardError();
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable stdErrScope = stdErr.ConfigureAwait(false);

    // Configure command:
    // - stdout is captured (for the result)
    // - stderr goes to console (for interactive UI)
    // - stdin comes from pipeline or was already configured
    Command interactiveCommand = InternalCommand
      .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputBuilder))
      .WithStandardErrorPipe(PipeTarget.ToStream(stdErr));

    CliExecution execution;
    try
    {
      execution = await ExecuteCliAsync(interactiveCommand, cancellationToken).ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
      // Cancellation must remain observable so callers can distinguish
      // "user cancelled" from "user selected nothing"
      throw;
    }
    catch (CliWrap.Exceptions.CommandExecutionException)
    {
      // Validation / command-execution failures propagate; only TtyPassthroughAsync is exempt.
      throw;
    }
    catch
    {
      // Graceful degradation - return empty string on failure
      return string.Empty;
    }

    CommandOutput captured = FinishCli(
      execution,
      outputBuilder.ToString(),
      string.Empty,
      throwOnStrictTimeout: true);
    return captured.Stdout.TrimEnd('\n', '\r');
  }

  /// <summary>
  /// Chains this command's stdout into another command, shell-pipe style.
  /// The pipeline keeps this command's timeout, grace period, and validation.
  /// Composition never throws: an invalid stage yields a command that reports
  /// <see cref="NeverRanExitCode"/> when executed. Pipe compositions bypass
  /// <see cref="Testing.CommandMock"/> matching (use loose mode when testing pipelines).
  /// </summary>
  /// <param name="executable">The next command in the pipeline</param>
  /// <param name="arguments">Arguments for the next command</param>
  /// <returns>A CommandResult representing the composed pipeline</returns>
  public CommandResult Pipe
  (
    string executable,
    params string[]? arguments
  )
  {
    return PipeCore(executable, arguments, options: null);
  }

  /// <summary>
  /// Chains this command's stdout into another command and applies <paramref name="options"/>
  /// to that stage (working directory, environment, validation, and timeout).
  /// The pipeline uses the shorter timeout. Its grace period comes from that shorter side.
  /// Equal timeouts keep the smaller grace period. Zero-exit validation on either stage makes the pipeline strict.
  /// Composition never throws: an invalid stage yields a command that reports
  /// <see cref="NeverRanExitCode"/> when executed. Pipe compositions bypass
  /// <see cref="Testing.CommandMock"/> matching (use loose mode when testing pipelines).
  /// Upstream configured standard input stays the pipeline stdin.
  /// </summary>
  /// <param name="executable">The next command in the pipeline</param>
  /// <param name="options">Options applied to the next stage</param>
  /// <param name="arguments">Arguments for the next command</param>
  /// <returns>A CommandResult representing the composed pipeline</returns>
  public CommandResult Pipe
  (
    string executable,
    CommandOptions options,
    params string[]? arguments
  )
  {
    ArgumentNullException.ThrowIfNull(options);
    return PipeCore(executable, arguments, options);
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI composition boundary: invalid or unavailable pipeline commands intentionally degrade to NullCommandResult instead of throwing."
  )]
  private CommandResult PipeCore
  (
    string executable,
    string[]? arguments,
    CommandOptions? options
  )
  {
    // Input validation
    if (InternalCommand == null)
    {
      return NullCommandResult;
    }

    if (string.IsNullOrWhiteSpace(executable))
    {
      return NullCommandResult;
    }

    try
    {
      // Use Run() to create the next command instead of duplicating logic
      CommandResult nextCommandResult = options is null
        ? CommandExtensions.Run(executable, arguments)
        : CommandExtensions.Run(executable, arguments, options);

      // If Run() failed, it returned a CommandResult with null Command
      if (nextCommandResult.InternalCommand == null)
      {
        return NullCommandResult;
      }

      // Chain commands using CliWrap's pipe operator
      Command pipedCommand = InternalCommand | nextCommandResult.InternalCommand;

      TimeSpan? stageTimeout = options?.Timeout;
      TimeSpan? sharedTimeout = MergeTimeout(Timeout, stageTimeout);
      TimeSpan sharedGrace = options is null
        ? TimeoutGracePeriod
        : GraceForSharedTimeout(Timeout, TimeoutGracePeriod, stageTimeout, options.TimeoutGracePeriod, sharedTimeout);
      CommandResultValidation sharedValidation = options is null
        ? Validation
        : MergeValidation(Validation, options.Validation);

      return new CommandResult(
        pipedCommand,
        ConfiguredStandardInput,
        isPipeline: true,
        mockExecutable: null,
        mockArguments: null,
        sharedTimeout,
        sharedGrace,
        sharedValidation);
    }
    catch
    {
      // Command creation failures return null command (graceful degradation)
      return NullCommandResult;
    }
  }

  /// <summary>
  /// Returns the command string that would be executed, useful for debugging.
  /// </summary>
  /// <returns>The command string in the format "executable arguments", or "[No command]" if no command is configured</returns>
  public string ToCommandString() => InternalCommand?.ToString() ?? "[No command]";

  /// <summary>
  /// Executes the command and streams output to the console in real-time.
  /// This is the default behavior matching shell execution (80% use case).
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The exit code of the command</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout.</exception>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return Remember(CommandOutput.Empty(NeverRanExitCode)).ExitCode;
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      await WriteMockToTerminalAsync(setup).ConfigureAwait(false);
      return CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: false).ExitCode;
    }

    // Stream to terminal using CliWrap's pipe targets
    Command consoleCommand = InternalCommand
      .WithStandardOutputPipe(PipeTarget.ToDelegate(line => TimeWarpTerminal.Default.WriteLine(line)))
      .WithStandardErrorPipe(PipeTarget.ToDelegate(line => TimeWarpTerminal.Default.WriteErrorLine(line)));

    CliExecution execution = await ExecuteCliAsync(consoleCommand, cancellationToken).ConfigureAwait(false);
    return FinishCli(execution, string.Empty, string.Empty, throwOnStrictTimeout: true).ExitCode;
  }

  /// <summary>
  /// Executes the command, streams output to console AND captures it.
  /// Useful for debugging/logging scenarios where you want to see output and save it.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout.</exception>
  public async Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return Remember(CommandOutput.Empty(NeverRanExitCode));
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      await WriteMockToTerminalAsync(setup).ConfigureAwait(false);
      return CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: true);
    }

    // Use StringBuilders to capture output while also streaming to console
    StringBuilder stdOutBuilder = new();
    StringBuilder stdErrBuilder = new();

    // Create pipe targets that both stream to terminal AND capture
    var stdOutTarget = PipeTarget.Merge(
      PipeTarget.ToDelegate(line => TimeWarpTerminal.Default.WriteLine(line)),
      PipeTarget.ToStringBuilder(stdOutBuilder)
    );

    var stdErrTarget = PipeTarget.Merge(
      PipeTarget.ToDelegate(line => TimeWarpTerminal.Default.WriteErrorLine(line)),
      PipeTarget.ToStringBuilder(stdErrBuilder)
    );

    Command captureCommand = InternalCommand
      .WithStandardOutputPipe(stdOutTarget)
      .WithStandardErrorPipe(stdErrTarget);

    CliExecution execution = await ExecuteCliAsync(captureCommand, cancellationToken).ConfigureAwait(false);
    return FinishCli(execution, stdOutBuilder.ToString(), stdErrBuilder.ToString(), throwOnStrictTimeout: true);
  }

  /// <summary>
  /// Executes the command silently and captures all output.
  /// No output is written to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout.</exception>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return Remember(CommandOutput.Empty(NeverRanExitCode));
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      return CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: true);
    }

    // Capture both stdout and stderr with timestamps
    List<OutputLine> outputLines = [];
    Lock outputLock = new();

    Command captureCommand = InternalCommand
      .WithStandardOutputPipe(PipeTarget.ToDelegate(line =>
      {
        using (outputLock.EnterScope())
        {
          outputLines.Add(new OutputLine(line, false));
        }
      }))
      .WithStandardErrorPipe(PipeTarget.ToDelegate(line =>
      {
        using (outputLock.EnterScope())
        {
          outputLines.Add(new OutputLine(line, true));
        }
      }));

    CliExecution execution = await ExecuteCliAsync(captureCommand, cancellationToken).ConfigureAwait(false);
    return FinishCli(execution, outputLines, throwOnStrictTimeout: true);
  }

  /// <summary>
  /// Executes the command and streams stdout lines without buffering.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>An async enumerable of stdout lines</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout. <see cref="LastOutput"/> is set before the exception.</exception>
  public async IAsyncEnumerable<string> StreamStdoutAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      Remember(CommandOutput.Empty(NeverRanExitCode));
      yield break;
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      foreach (string line in SplitMockLines(setup.Stdout))
      {
        yield return line;
      }

      _ = CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: true);
      yield break;
    }

    await foreach (CommandEvent evt in ListenWithTimeoutAsync(cancellationToken).ConfigureAwait(false))
    {
      if (evt is StandardOutputCommandEvent stdOut)
      {
        yield return stdOut.Text;
      }
    }
  }

  /// <summary>
  /// Executes the command and streams stderr lines without buffering.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>An async enumerable of stderr lines</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout. <see cref="LastOutput"/> is set before the exception.</exception>
  public async IAsyncEnumerable<string> StreamStderrAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      Remember(CommandOutput.Empty(NeverRanExitCode));
      yield break;
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      foreach (string line in SplitMockLines(setup.Stderr))
      {
        yield return line;
      }

      _ = CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: true);
      yield break;
    }

    await foreach (CommandEvent evt in ListenWithTimeoutAsync(cancellationToken).ConfigureAwait(false))
    {
      if (evt is StandardErrorCommandEvent stdErr)
      {
        yield return stdErr.Text;
      }
    }
  }

  /// <summary>
  /// Executes the command and streams combined output with source information.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>An async enumerable of OutputLine objects</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout. <see cref="LastOutput"/> is set before the exception.</exception>
  public async IAsyncEnumerable<OutputLine> StreamCombinedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      Remember(CommandOutput.Empty(NeverRanExitCode));
      yield break;
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      foreach (string line in SplitMockLines(setup.Stdout))
      {
        yield return new OutputLine(line, false);
      }

      foreach (string line in SplitMockLines(setup.Stderr))
      {
        yield return new OutputLine(line, true);
      }

      _ = CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: true);
      yield break;
    }

    await foreach (CommandEvent evt in ListenWithTimeoutAsync(cancellationToken).ConfigureAwait(false))
    {
      if (evt is StandardOutputCommandEvent stdOut)
      {
        yield return new OutputLine(stdOut.Text, false);
      }
      else if (evt is StandardErrorCommandEvent stdErr)
      {
        yield return new OutputLine(stdErr.Text, true);
      }
    }
  }

  /// <summary>
  /// Executes the command and streams output directly to a file without buffering.
  /// </summary>
  /// <param name="filePath">Path to the output file</param>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>A task that completes when the command finishes</returns>
  /// <exception cref="TimeoutException">Strict validation is set and the command exceeded its timeout.</exception>
  public async Task StreamToFileAsync(string filePath, CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      Remember(CommandOutput.Empty(NeverRanExitCode));
      return;
    }

    MockExecution mock = await TryExecuteMockAsync(cancellationToken).ConfigureAwait(false);
    if (mock is { Handled: true, Setup: { } setup })
    {
      await File.WriteAllTextAsync(
        filePath,
        (setup.Stdout ?? string.Empty) + (setup.Stderr ?? string.Empty),
        cancellationToken).ConfigureAwait(false);
      _ = CompleteMock(setup, mock.TimedOut, throwOnStrictTimeout: true, includeText: true);
      return;
    }

    FileStream fileStream = File.Create(filePath);
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable fileStreamScope = fileStream.ConfigureAwait(false);
    StreamWriter writer = new(fileStream);
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable writerScope = writer.ConfigureAwait(false);

    // CliWrap pumps stdout and stderr concurrently; two PipeTarget.ToStream targets
    // sharing one FileStream race and corrupt the file (FileStream is not thread-safe).
    // Serialize line writes under a lock instead; interleaving granularity is one line.
    Lock writeLock = new();

    Command fileCommand = InternalCommand
      .WithStandardOutputPipe(PipeTarget.ToDelegate(line =>
      {
        using (writeLock.EnterScope())
        {
          writer.WriteLine(line);
        }
      }))
      .WithStandardErrorPipe(PipeTarget.ToDelegate(line =>
      {
        using (writeLock.EnterScope())
        {
          writer.WriteLine(line);
        }
      }));

    CliExecution execution = await ExecuteCliAsync(fileCommand, cancellationToken).ConfigureAwait(false);
    _ = FinishCli(execution, string.Empty, string.Empty, throwOnStrictTimeout: true);
  }
}
