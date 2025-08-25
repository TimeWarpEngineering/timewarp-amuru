namespace TimeWarp.Amuru;

public class CommandResult
{
  private readonly Command? Command;
  
  // Constants for line splitting - handle both Unix (\n) and Windows (\r\n) line endings
  private static readonly char[] NewlineCharacters = { '\n', '\r' };
  
  // Singleton for failed commands to avoid creating multiple identical null instances
  internal static readonly CommandResult NullCommandResult = new(null);
  
  // Property to access Command from other CommandResult instances in Pipe() method
  private Command? InternalCommand => Command;
  
  
  internal CommandResult(Command? command)
  {
    Command = command;
  }
  
  
  public async Task<string> GetStringAsync(CancellationToken cancellationToken = default)
  {
    if (Command == null)
    {
      return string.Empty;
    }
    
    BufferedCommandResult result = await Command.ExecuteBufferedAsync(cancellationToken);
    return result.StandardOutput;
  }
  
  public async Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default)
  {
    if (Command == null)
    {
      return [];
    }
    
    BufferedCommandResult result = await Command.ExecuteBufferedAsync(cancellationToken);
    return result.StandardOutput.Split
    (
      NewlineCharacters, 
      StringSplitOptions.RemoveEmptyEntries
    );
  }
  
  public async Task<ExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default)
  {
    if (Command == null)
    {
      return new ExecutionResult(
        new CliWrap.CommandResult(0, DateTimeOffset.MinValue, DateTimeOffset.MinValue),
        string.Empty,
        string.Empty
      );
    }
    
    // Use ExecuteBufferedAsync to capture stdout and stderr
    BufferedCommandResult bufferedResult = await Command.ExecuteBufferedAsync(cancellationToken);
    
    // Create our result with captured output
    var result = new ExecutionResult(
      new CliWrap.CommandResult(
        bufferedResult.ExitCode,
        bufferedResult.StartTime,
        bufferedResult.ExitTime
      ),
      bufferedResult.StandardOutput,
      bufferedResult.StandardError
    );
    
    return result;
  }
  
  /// <summary>
  /// Executes the command with stdin, stdout, and stderr connected to the console for interactive use.
  /// This allows commands like fzf to work with user input and terminal UI.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<ExecutionResult> ExecuteInteractiveAsync(CancellationToken cancellationToken = default)
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
  /// Executes the command interactively while capturing the output.
  /// This is ideal for commands like fzf where the UI is rendered to stderr but the selection is written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The captured output string from the interactive command</returns>
  public async Task<string> GetStringInteractiveAsync(CancellationToken cancellationToken = default)
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
}