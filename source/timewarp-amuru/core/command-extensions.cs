#region Purpose
// Provides helper methods for executing shell commands via CliWrap
// Handles special cases like C# scripts which require -- argument prefixing
#endregion

#region Design
// - Static helper class providing Run() overloads for command execution
// - Special handling for .cs files: inserts "--" to prevent dotnet CLI interception
// - Command path overrides via CliConfiguration for tool aliasing
// - Returns CommandResult for fluent builder pattern continuation
// - Supports standard input piping for interactive commands
#endregion

namespace TimeWarp.Amuru;

internal static class CommandExtensions
{
  private const string CSharpScriptExtension = ".cs";
  
  internal static CommandResult Run
  (
    string executable,
    params string[]? arguments
  )
  {
    return Run(executable, arguments, new CommandOptions());
  }
  
  internal static CommandResult Run
  (
    string executable,
    string[]? arguments,
    CommandOptions commandOptions,
    string? standardInput = null
  )
  {
    // Input validation
    if (string.IsNullOrWhiteSpace(executable))
    {
      return CommandResult.NullCommandResult;
    }
    
    if (commandOptions == null)
    {
      return CommandResult.NullCommandResult;
    }
    
    // Check for configured command path override
    executable = CliConfiguration.GetCommandPath(executable);
    
    // Handle .cs script files specially
    if (executable.EndsWith(CSharpScriptExtension, StringComparison.OrdinalIgnoreCase))
    {
      // Insert -- at the beginning of arguments to prevent dotnet from intercepting them
      List<string> newArgs = ["--", .. (arguments ?? [])];
      arguments = [.. newArgs];
    }
    
    Command cliCommand = CliWrap.Cli.Wrap(executable)
      .WithArguments(arguments ?? []);
    
    // Apply configuration options
    cliCommand = commandOptions.ApplyTo(cliCommand);
    
    // Apply standard input if provided
    if (!string.IsNullOrEmpty(standardInput))
    {
      cliCommand = cliCommand.WithStandardInputPipe(PipeSource.FromString(standardInput));
    }
      
    return new CommandResult(cliCommand);
  }
}