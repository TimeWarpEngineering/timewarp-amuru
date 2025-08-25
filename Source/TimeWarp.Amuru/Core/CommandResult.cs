namespace TimeWarp.Amuru;

public class CommandResult
{
  private readonly Command? Command;
  
  // Singleton for failed commands to avoid creating multiple identical null instances
  internal static readonly CommandResult NullCommandResult = new(null);
  
  // Property to access Command from other CommandResult instances in Pipe() method
  private Command? InternalCommand => Command;
  
  
  internal CommandResult(Command? command)
  {
    Command = command;
  }
  
  
  /// <summary>
  /// Passes the command through to the terminal with full interactive control.
  /// This allows commands like vim, fzf, or REPLs to work with user input and terminal UI.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    if (Command == null)
    {
      return new ExecutionResult(
        new CliWrap.CommandResult(0, DateTimeOffset.MinValue, DateTimeOffset.MinValue),
        string.Empty,
        string.Empty
      );
    }
    
    // Open console streams
    await using Stream stdIn = Console.OpenStandardInput();
    await using Stream stdOut = Console.OpenStandardOutput();
    await using Stream stdErr = Console.OpenStandardError();
    
    // Configure command with console pipes
    Command interactiveCommand = Command
      .WithStandardInputPipe(PipeSource.FromStream(stdIn))
      .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
      .WithStandardErrorPipe(PipeTarget.ToStream(stdErr));
    
    // Execute interactively
    CliWrap.CommandResult result = await interactiveCommand.ExecuteAsync(cancellationToken);
    
    // Return result with empty output strings (output went to console)
    return new ExecutionResult(
      result,
      string.Empty,
      string.Empty
    );
  }
  
  /// <summary>
  /// Executes an interactive selection command and returns the selected value.
  /// This is ideal for commands like fzf where the UI is rendered to stderr but the selection is written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected value from the interactive command</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    if (Command == null)
    {
      return string.Empty;
    }
    
    // Use StringBuilder to capture output
    StringBuilder outputBuilder = new();
    await using Stream stdErr = Console.OpenStandardError();
    
    // Configure command:
    // - stdout is captured (for the result)
    // - stderr goes to console (for interactive UI)
    // - stdin comes from pipeline or was already configured
    Command interactiveCommand = Command
      .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputBuilder))
      .WithStandardErrorPipe(PipeTarget.ToStream(stdErr));
    
    try
    {
      await interactiveCommand.ExecuteAsync(cancellationToken);
    }
    catch
    {
      // Graceful degradation - return empty string on failure
      return string.Empty;
    }
    
    return outputBuilder.ToString().TrimEnd('\n', '\r');
  }
  
  public CommandResult Pipe
  (
    string executable,
    params string[]? arguments
  )
  {
    // Input validation
    if (Command == null)
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
      Command pipedCommand = Command | nextCommandResult.InternalCommand;
      
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
  public string ToCommandString() => Command?.ToString() ?? "[No command]";

  // Temporary compatibility methods - will be removed after builders are updated
  [Obsolete("Use CaptureAsync().Result.Stdout instead")]
  public async Task<string> GetStringAsync(CancellationToken cancellationToken = default)
  {
    CommandOutput result = await CaptureAsync(cancellationToken);
    return result.Stdout;
  }

  [Obsolete("Use CaptureAsync().Result.GetLines() instead")]
  public async Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default)
  {
    CommandOutput result = await CaptureAsync(cancellationToken);
    return result.GetLines();
  }

  [Obsolete("Use CaptureAsync() instead")]
  public async Task<ExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default)
  {
    CommandOutput result = await CaptureAsync(cancellationToken);
    return new ExecutionResult(
      new CliWrap.CommandResult(result.ExitCode, DateTimeOffset.MinValue, DateTimeOffset.MinValue),
      result.Stdout,
      result.Stderr
    );
  }

  [Obsolete("Use PassthroughAsync() instead")]
  public async Task<ExecutionResult> ExecuteInteractiveAsync(CancellationToken cancellationToken = default)
  {
    return await PassthroughAsync(cancellationToken);
  }

  [Obsolete("Use SelectAsync() instead")]
  public async Task<string> GetStringInteractiveAsync(CancellationToken cancellationToken = default)
  {
    return await SelectAsync(cancellationToken);
  }

  /// <summary>
  /// Executes the command and streams output to the console in real-time.
  /// This is the default behavior matching shell execution (80% use case).
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The exit code of the command</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    if (Command == null)
    {
      return 0;
    }

    // Stream to console using CliWrap's pipe targets
    Command consoleCommand = Command
      .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
      .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine));

    CliWrap.CommandResult result = await consoleCommand.ExecuteAsync(cancellationToken);
    return result.ExitCode;
  }

  /// <summary>
  /// Executes the command silently and captures all output.
  /// No output is written to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    if (Command == null)
    {
      return CommandOutput.Empty();
    }

    // Capture both stdout and stderr with timestamps
    List<OutputLine> outputLines = [];
    Lock outputLock = new();

    Command captureCommand = Command
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
    if (Command == null)
    {
      yield break;
    }

    // Use CliWrap's event stream for stdout
    await foreach (CommandEvent evt in Command.ListenAsync(cancellationToken))
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
    if (Command == null)
    {
      yield break;
    }

    // Use CliWrap's event stream for stderr
    await foreach (CommandEvent evt in Command.ListenAsync(cancellationToken))
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
    if (Command == null)
    {
      yield break;
    }

    // Use CliWrap's event stream for combined output
    await foreach (CommandEvent evt in Command.ListenAsync(cancellationToken))
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
    if (Command == null)
    {
      return;
    }

    await using FileStream fileStream = File.Create(filePath);
    
    Command fileCommand = Command
      .WithStandardOutputPipe(PipeTarget.ToStream(fileStream))
      .WithStandardErrorPipe(PipeTarget.ToStream(fileStream));

    await fileCommand.ExecuteAsync(cancellationToken);
  }
}