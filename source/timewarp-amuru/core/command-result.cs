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
// - Streaming methods use CliWrap event streams for low-buffer processing.
// - Pipe composes commands by building the next stage and using CliWrap's pipe operator.
// - Mock support applies to ALL execution modes (run, capture, stream, select, passthrough, TTY).
//   Strict mode (the default) throws on unmocked commands so tests can never silently run real processes.
//   Pipe compositions are the exception: they bypass mock matching (use MockBehavior.Loose for pipelines).
// - Null commands never throw, preserving shell-like composition, but they report FAILURE:
//   NeverRanExitCode (-1) via ExitCode/Success so a command that never ran is distinguishable from one that succeeded.
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

public class CommandResult
{
  /// <summary>
  /// Exit code reported when a command never ran (invalid construction, failed pipe composition).
  /// Distinguishes never-ran from a successful (0) or tool-reported non-zero exit.
  /// </summary>
  public const int NeverRanExitCode = -1;

  // Singleton for failed commands to avoid creating multiple identical null instances
  internal static readonly CommandResult NullCommandResult = new(null);

  // Property to access Command from other CommandResult instances in Pipe() method
  private Command? InternalCommand { get; }

  // Original executable/arguments as provided at construction, used for mock matching.
  // CliWrap's Arguments property is an escaped string; re-splitting it breaks on arguments
  // containing spaces, so the raw array must be carried through (null for piped compositions).
  private string? MockExecutable { get; }
  private string[]? MockArguments { get; }

  internal CommandResult(Command? command)
  {
    InternalCommand = command;
  }

  internal CommandResult(Command? command, string executable, string[] arguments) : this(command)
  {
    MockExecutable = executable;
    MockArguments = arguments;
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

    string executable = MockExecutable ?? InternalCommand.TargetFilePath;
    string[] arguments = MockArguments ?? InternalCommand.Arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (state.TryGetSetup(executable, arguments, out Testing.MockSetupData? setupData) && setupData != null)
    {
      state.RecordCall(executable, arguments);
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
    string.IsNullOrEmpty(text) ? [] : text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

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
  /// Caveat: the child's stdin is piped from the console. If the child exits without draining
  /// stdin, a pending console read can remain and swallow the parent's next line of input.
  /// Prefer TtyPassthroughAsync (stream inheritance, no pending reads) when the child may
  /// ignore stdin.
  /// </para>
  /// </remarks>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return CommandOutput.Empty(NeverRanExitCode);
    }

    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);
      if (!string.IsNullOrEmpty(mockSetup.Stdout))
      {
        await TimeWarpTerminal.Default.WriteLineAsync(mockSetup.Stdout).ConfigureAwait(false);
      }

      if (!string.IsNullOrEmpty(mockSetup.Stderr))
      {
        await TimeWarpTerminal.Default.WriteErrorLineAsync(mockSetup.Stderr).ConfigureAwait(false);
      }

      return new CommandOutput(string.Empty, string.Empty, mockSetup.ExitCode);
    }

    // Open console streams for interactive piping
    Stream stdIn = TimeWarpTerminal.Default.OpenStandardInput();
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable stdInScope = stdIn.ConfigureAwait(false);
    Stream stdOut = TimeWarpTerminal.Default.OpenStandardOutput();
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable stdOutScope = stdOut.ConfigureAwait(false);
    Stream stdErr = TimeWarpTerminal.Default.OpenStandardError();
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable stdErrScope = stdErr.ConfigureAwait(false);

    // Configure command with console pipes
    Command interactiveCommand = InternalCommand
      .WithStandardInputPipe(PipeSource.FromStream(stdIn))
      .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
      .WithStandardErrorPipe(PipeTarget.ToStream(stdErr));

    // Execute interactively
    CliWrap.CommandResult result = await interactiveCommand.ExecuteAsync(cancellationToken);

    // Return result with empty output strings (output went to console)
    return new CommandOutput(string.Empty, string.Empty, result.ExitCode) { RunTime = result.RunTime };
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
  /// Validation options do not apply here: this method never throws on a
  /// non-zero exit code (even with WithZeroExitCodeValidation); inspect
  /// the returned CommandOutput's ExitCode/Success instead.
  /// </para>
  /// </remarks>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since streams are inherited)</returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI boundary: cancellation/teardown race can throw during process kill; failures are intentionally ignored to preserve graceful shutdown."
  )]
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return CommandOutput.Empty(NeverRanExitCode);
    }

    Testing.MockSetupData? ttyMockSetup = ResolveMockSetup();
    if (ttyMockSetup != null)
    {
      await ApplyMockPreludeAsync(ttyMockSetup, cancellationToken).ConfigureAwait(false);
      return new CommandOutput(string.Empty, string.Empty, ttyMockSetup.ExitCode);
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

    // Register cancellation
    CancellationTokenRegistration registration = cancellationToken.Register(() =>
    {
      try
      {
        process.Kill(entireProcessTree: true);
      }
      catch
      {
        // Process may have already exited
      }
    });
    await using System.Runtime.CompilerServices.ConfiguredAsyncDisposable registrationScope = registration.ConfigureAwait(false);

    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

    DateTimeOffset exitTime = DateTimeOffset.Now;

    return new CommandOutput(string.Empty, string.Empty, process.ExitCode) { RunTime = exitTime - startTime };
  }

  /// <summary>
  /// Executes an interactive selection command and returns the selected value.
  /// This is ideal for commands like fzf where the UI is rendered to stderr but the selection is written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected value from the interactive command</returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI boundary: interactive selection should degrade gracefully and return empty result on unexpected runtime failures."
  )]
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return string.Empty;
    }

    // Mock resolution happens outside the graceful-degradation try below so strict-mode
    // violations and configured mock exceptions propagate to the test instead of being swallowed.
    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);
      return (mockSetup.Stdout ?? string.Empty).TrimEnd('\n', '\r');
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

    try
    {
      await interactiveCommand.ExecuteAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
      // Cancellation must remain observable so callers can distinguish
      // "user cancelled" from "user selected nothing"
      throw;
    }
    catch
    {
      // Graceful degradation - return empty string on failure
      return string.Empty;
    }

    return outputBuilder.ToString().TrimEnd('\n', '\r');
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI composition boundary: invalid or unavailable pipeline commands intentionally degrade to NullCommandResult instead of throwing."
  )]
  public CommandResult Pipe
  (
    string executable,
    params string[]? arguments
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
      CommandResult nextCommandResult = CommandExtensions.Run(executable, arguments);

      // If Run() failed, it returned a CommandResult with null Command
      if (nextCommandResult.InternalCommand == null)
      {
        return NullCommandResult;
      }

      // Chain commands using CliWrap's pipe operator
      Command pipedCommand = InternalCommand | nextCommandResult.InternalCommand;

      return new CommandResult(pipedCommand);
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
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return NeverRanExitCode;
    }

    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);

      // Write mock output to terminal to simulate RunAsync behavior
      if (!string.IsNullOrEmpty(mockSetup.Stdout))
      {
        await TimeWarpTerminal.Default.WriteLineAsync(mockSetup.Stdout).ConfigureAwait(false);
      }

      if (!string.IsNullOrEmpty(mockSetup.Stderr))
      {
        await TimeWarpTerminal.Default.WriteErrorLineAsync(mockSetup.Stderr).ConfigureAwait(false);
      }

      return mockSetup.ExitCode;
    }

    // Stream to terminal using CliWrap's pipe targets
    Command consoleCommand = InternalCommand
      .WithStandardOutputPipe(PipeTarget.ToDelegate(line => TimeWarpTerminal.Default.WriteLine(line)))
      .WithStandardErrorPipe(PipeTarget.ToDelegate(line => TimeWarpTerminal.Default.WriteErrorLine(line)));

    CliWrap.CommandResult result = await consoleCommand.ExecuteAsync(cancellationToken);
    return result.ExitCode;
  }

  /// <summary>
  /// Executes the command, streams output to console AND captures it.
  /// Useful for debugging/logging scenarios where you want to see output and save it.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public async Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return CommandOutput.Empty(NeverRanExitCode);
    }

    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);

      // Write to terminal for RunAndCapture behavior
      if (!string.IsNullOrEmpty(mockSetup.Stdout))
      {
        await TimeWarpTerminal.Default.WriteLineAsync(mockSetup.Stdout).ConfigureAwait(false);
      }

      if (!string.IsNullOrEmpty(mockSetup.Stderr))
      {
        await TimeWarpTerminal.Default.WriteErrorLineAsync(mockSetup.Stderr).ConfigureAwait(false);
      }

      return new CommandOutput(mockSetup.Stdout ?? string.Empty, mockSetup.Stderr ?? string.Empty, mockSetup.ExitCode);
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

    CliWrap.CommandResult result = await captureCommand.ExecuteAsync(cancellationToken);

    return new CommandOutput(
      stdOutBuilder.ToString(),
      stdErrBuilder.ToString(),
      result.ExitCode
    );
  }

  /// <summary>
  /// Executes the command silently and captures all output.
  /// No output is written to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return CommandOutput.Empty(NeverRanExitCode);
    }

    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);
      return new CommandOutput(mockSetup.Stdout ?? string.Empty, mockSetup.Stderr ?? string.Empty, mockSetup.ExitCode);
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

    CliWrap.CommandResult result = await captureCommand.ExecuteAsync(cancellationToken);
    return new CommandOutput(outputLines, result.ExitCode);
  }

  /// <summary>
  /// Executes the command and streams stdout lines without buffering.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>An async enumerable of stdout lines</returns>
  public async IAsyncEnumerable<string> StreamStdoutAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      yield break;
    }

    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);
      foreach (string line in SplitMockLines(mockSetup.Stdout))
      {
        yield return line;
      }

      yield break;
    }

    // Use CliWrap's event stream for stdout
    await foreach (CommandEvent evt in InternalCommand.ListenAsync(cancellationToken).ConfigureAwait(false))
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
  public async IAsyncEnumerable<string> StreamStderrAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      yield break;
    }

    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);
      foreach (string line in SplitMockLines(mockSetup.Stderr))
      {
        yield return line;
      }

      yield break;
    }

    // Use CliWrap's event stream for stderr
    await foreach (CommandEvent evt in InternalCommand.ListenAsync(cancellationToken).ConfigureAwait(false))
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
  public async IAsyncEnumerable<OutputLine> StreamCombinedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      yield break;
    }

    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);
      foreach (string line in SplitMockLines(mockSetup.Stdout))
      {
        yield return new OutputLine(line, false);
      }

      foreach (string line in SplitMockLines(mockSetup.Stderr))
      {
        yield return new OutputLine(line, true);
      }

      yield break;
    }

    // Use CliWrap's event stream for combined output
    await foreach (CommandEvent evt in InternalCommand.ListenAsync(cancellationToken).ConfigureAwait(false))
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
  public async Task StreamToFileAsync(string filePath, CancellationToken cancellationToken = default)
  {
    if (InternalCommand == null)
    {
      return;
    }

    Testing.MockSetupData? mockSetup = ResolveMockSetup();
    if (mockSetup != null)
    {
      await ApplyMockPreludeAsync(mockSetup, cancellationToken).ConfigureAwait(false);
      await File.WriteAllTextAsync(filePath, (mockSetup.Stdout ?? string.Empty) + (mockSetup.Stderr ?? string.Empty), cancellationToken).ConfigureAwait(false);
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

    await fileCommand.ExecuteAsync(cancellationToken);
  }
}
